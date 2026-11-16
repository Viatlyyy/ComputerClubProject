BEGIN;
LOCK TABLE Sessions IN ACCESS EXCLUSIVE MODE;
ALTER TABLE Sessions ALTER COLUMN DurationMinutes TYPE DOUBLE PRECISION;
DO $migration$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'public' AND table_name = 'sessions' AND column_name = 'paidat'
    ) THEN
        ALTER TABLE Sessions ADD COLUMN PaidAt TIMESTAMP NULL;
        UPDATE Sessions SET PaidAt = COALESCE(EndTime, PlannedEndTime) WHERE IsCompleted;
    END IF;
END
$migration$;
ALTER TABLE Sessions ADD COLUMN IF NOT EXISTS ReceiptClientName TEXT NULL;
UPDATE Sessions s SET ReceiptClientName = c.FullName
FROM Clients c WHERE c.Id = s.ClientId AND s.IsCompleted AND s.ReceiptClientName IS NULL;
DO $constraints$
DECLARE existing_name TEXT;
BEGIN
    FOR existing_name IN
        SELECT conname FROM pg_constraint
        WHERE conrelid = 'public.sessions'::regclass
          AND confrelid = 'public.clients'::regclass AND contype = 'f'
    LOOP
        EXECUTE format('ALTER TABLE Sessions DROP CONSTRAINT %I', existing_name);
    END LOOP;
    ALTER TABLE Sessions ADD CONSTRAINT sessions_client_history_fk
        FOREIGN KEY (ClientId) REFERENCES Clients(Id) ON DELETE RESTRICT;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint
                   WHERE conrelid = 'public.sessions'::regclass AND conname = 'sessions_payment_completed') THEN
        ALTER TABLE Sessions ADD CONSTRAINT sessions_payment_completed CHECK (PaidAt IS NULL OR IsCompleted);
    END IF;
END
$constraints$;
CREATE INDEX IF NOT EXISTS sessions_unpaid_idx ON Sessions (EndTime, Id) WHERE IsCompleted AND PaidAt IS NULL;
CREATE INDEX IF NOT EXISTS sessions_paid_idx ON Sessions (PaidAt) WHERE PaidAt IS NOT NULL;
COMMIT;
