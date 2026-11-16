param(
    [Parameter(Mandatory)][ValidatePattern('\A[a-z][a-z0-9_]{0,62}\z')][string]$Database,
    [ValidateSet('backup','sql')][string]$Format = 'backup',
    [string]$PgBin = 'C:\Program Files\PostgreSQL\18\bin',
    [string]$Server = 'localhost',
    [ValidateRange(1,65535)][int]$Port = 5432,
    [string]$User = 'postgres'
)

$ErrorActionPreference = 'Stop'
$backupPath = Join-Path $PSScriptRoot 'ComputerClub.backup'
$sqlPath = Join-Path $PSScriptRoot 'ComputerClub_full.sql'
$paymentsPath = Join-Path $PSScriptRoot '001_payments.sql'
$filePath = if ($Format -eq 'backup') { $backupPath } else { $sqlPath }
if (!(Test-Path -LiteralPath $filePath -PathType Leaf)) { throw 'Файл базы не найден.' }
if (!(Test-Path -LiteralPath $paymentsPath -PathType Leaf)) { throw 'Файл обновления для истории чеков не найден.' }
$createdb = Join-Path $PgBin 'createdb.exe'
$pgRestore = Join-Path $PgBin 'pg_restore.exe'
$psql = Join-Path $PgBin 'psql.exe'
$importer = if ($Format -eq 'backup') { $pgRestore } else { $psql }
foreach ($tool in @($createdb, $importer, $psql)) {
    if (!(Test-Path -LiteralPath $tool -PathType Leaf)) { throw "Утилита PostgreSQL не найдена: $tool" }
}
$serverArgs = @("--host=$Server", "--port=$Port", "--username=$User")
& $createdb @serverArgs --encoding=UTF8 --template=template0 -- $Database
if ($LASTEXITCODE -ne 0) { throw 'Не удалось создать новую базу. Импорт не начат.' }
$targetArgs = $serverArgs + @("--dbname=$Database")
if ($Format -eq 'backup') {
    & $pgRestore @targetArgs --exit-on-error --single-transaction --no-owner --no-privileges --no-tablespaces -- $filePath
} else {
    & $psql -X @targetArgs --set=ON_ERROR_STOP=1 --single-transaction "--file=$filePath"
}
if ($LASTEXITCODE -ne 0) {
    throw "Импорт не удался. Проверьте ошибку выше; новая база $Database оставлена для проверки."
}
& $psql -X @targetArgs --set=ON_ERROR_STOP=1 "--file=$paymentsPath"
if ($LASTEXITCODE -ne 0) {
    throw "База $Database восстановлена, но обновление для истории чеков не применилось."
}
Write-Output "Импорт завершён: $Database. Назначьте локальные пароли приложения перед входом."
