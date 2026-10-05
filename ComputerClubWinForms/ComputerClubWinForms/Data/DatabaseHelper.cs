using System;
using System.Collections.Generic;
using Npgsql;

namespace ComputerClubWinForms.Data;

public class DatabaseHelper
{
    public string DatabaseName { get; }
    public string ConnectionString { get; }
    private string MasterConnectionString { get; }

    public DatabaseHelper()
    {
        var host = Environment.GetEnvironmentVariable("PGHOST");
        var portText = Environment.GetEnvironmentVariable("PGPORT");
        var database = Environment.GetEnvironmentVariable("PGDATABASE");
        var username = Environment.GetEnvironmentVariable("PGUSER");
        var password = Environment.GetEnvironmentVariable("PGPASSWORD");

        host = string.IsNullOrWhiteSpace(host) ? "localhost" : host;
        database = string.IsNullOrWhiteSpace(database) ? "computer_club" : database;
        username = string.IsNullOrWhiteSpace(username) ? "postgres" : username;
        password = string.IsNullOrWhiteSpace(password) ? "123" : password;
        var port = int.TryParse(portText, out var parsedPort) ? parsedPort : 5432;

        DatabaseName = database;
        MasterConnectionString = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Database = "postgres",
            Username = username,
            Password = password,
            IncludeErrorDetail = true
        }.ToString();

        ConnectionString = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Database = database,
            Username = username,
            Password = password,
            IncludeErrorDetail = true
        }.ToString();
    }

    public NpgsqlConnection CreateConnection()
    {
        var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();
        return connection;
    }

    public void Initialize()
    {
        EnsureDatabaseExists();
        using var connection = CreateConnection();
        ResetSchemaIfIncompatible(connection);
        CreateTables(connection);
        SeedInitialData(connection);
    }

    private void EnsureDatabaseExists()
    {
        using var connection = new NpgsqlConnection(MasterConnectionString);
        connection.Open();

        using var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = "SELECT 1 FROM pg_database WHERE datname = @database";
        checkCommand.Parameters.AddWithValue("@database", DatabaseName);

        if (checkCommand.ExecuteScalar() is not null)
            return;

        using var createCommand = connection.CreateCommand();
        createCommand.CommandText = "CREATE DATABASE " + QuoteIdentifier(DatabaseName);
        createCommand.ExecuteNonQuery();
    }

    private static string QuoteIdentifier(string value)
    {
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }


    private static void ResetSchemaIfIncompatible(NpgsqlConnection connection)
    {
        var requiredColumns = new Dictionary<string, string[]>
        {
            ["users"] = new[] { "id", "username", "password", "role" },
            ["clients"] = new[] { "id", "fullname", "phone", "totalhours", "totalspent", "discountpercent" },
            ["computers"] = new[] { "number", "name", "isactive" },
            ["bookings"] = new[] { "id", "clientid", "computernumber", "plannedstart", "durationminutes", "status", "createdat" },
            ["sessions"] = new[] { "id", "clientid", "computernumber", "starttime", "plannedendtime", "endtime", "durationminutes", "totalcost", "tariffperhour", "onetimediscountpercent", "personaldiscountpercent", "iscompleted", "bookingid" },
            ["settings"] = new[] { "settingkey", "value" }
        };

        var incompatible = false;
        foreach (var pair in requiredColumns)
        {
            if (!TableExists(connection, pair.Key))
                continue;

            foreach (var column in pair.Value)
            {
                if (ColumnExists(connection, pair.Key, column))
                    continue;

                incompatible = true;
                break;
            }

            if (incompatible)
                break;
        }

        if (!incompatible)
            return;

        using var command = connection.CreateCommand();
        command.CommandText = """
        DROP TABLE IF EXISTS Sessions CASCADE;
        DROP TABLE IF EXISTS Bookings CASCADE;
        DROP TABLE IF EXISTS Settings CASCADE;
        DROP TABLE IF EXISTS Computers CASCADE;
        DROP TABLE IF EXISTS Clients CASCADE;
        DROP TABLE IF EXISTS Users CASCADE;
        """;
        command.ExecuteNonQuery();
    }

    private static bool TableExists(NpgsqlConnection connection, string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = 'public' AND table_name = @tableName
            )
            """;
        command.Parameters.AddWithValue("@tableName", tableName);
        return Convert.ToBoolean(command.ExecuteScalar());
    }

    private static bool ColumnExists(NpgsqlConnection connection, string tableName, string columnName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.columns
                WHERE table_schema = 'public' AND table_name = @tableName AND column_name = @columnName
            )
            """;
        command.Parameters.AddWithValue("@tableName", tableName);
        command.Parameters.AddWithValue("@columnName", columnName);
        return Convert.ToBoolean(command.ExecuteScalar());
    }

    private static void CreateTables(NpgsqlConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
        CREATE TABLE IF NOT EXISTS Users (
            Id SERIAL PRIMARY KEY,
            Username TEXT NOT NULL UNIQUE,
            Password TEXT NOT NULL,
            Role TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS Clients (
            Id SERIAL PRIMARY KEY,
            FullName TEXT NOT NULL,
            Phone TEXT NOT NULL UNIQUE,
            TotalHours DOUBLE PRECISION NOT NULL DEFAULT 0,
            TotalSpent NUMERIC(12, 2) NOT NULL DEFAULT 0,
            DiscountPercent INTEGER NOT NULL DEFAULT 0
        );

        CREATE TABLE IF NOT EXISTS Computers (
            Number INTEGER PRIMARY KEY,
            Name TEXT NOT NULL,
            IsActive BOOLEAN NOT NULL DEFAULT TRUE
        );

        CREATE TABLE IF NOT EXISTS Bookings (
            Id SERIAL PRIMARY KEY,
            ClientId INTEGER NOT NULL,
            ComputerNumber INTEGER NOT NULL,
            PlannedStart TIMESTAMP NOT NULL,
            DurationMinutes INTEGER NOT NULL,
            Status TEXT NOT NULL DEFAULT 'active',
            CreatedAt TIMESTAMP NOT NULL,
            FOREIGN KEY (ClientId) REFERENCES Clients(Id) ON DELETE CASCADE,
            FOREIGN KEY (ComputerNumber) REFERENCES Computers(Number)
        );

        CREATE TABLE IF NOT EXISTS Sessions (
            Id SERIAL PRIMARY KEY,
            ClientId INTEGER NOT NULL,
            ComputerNumber INTEGER NOT NULL,
            StartTime TIMESTAMP NOT NULL,
            PlannedEndTime TIMESTAMP NOT NULL,
            EndTime TIMESTAMP NULL,
            DurationMinutes INTEGER NOT NULL DEFAULT 0,
            TotalCost NUMERIC(12, 2) NOT NULL DEFAULT 0,
            TariffPerHour NUMERIC(12, 2) NOT NULL DEFAULT 0,
            OneTimeDiscountPercent NUMERIC(5, 2) NOT NULL DEFAULT 0,
            PersonalDiscountPercent NUMERIC(5, 2) NOT NULL DEFAULT 0,
            IsCompleted BOOLEAN NOT NULL DEFAULT FALSE,
            BookingId INTEGER NULL,
            FOREIGN KEY (ClientId) REFERENCES Clients(Id) ON DELETE CASCADE,
            FOREIGN KEY (ComputerNumber) REFERENCES Computers(Number),
            FOREIGN KEY (BookingId) REFERENCES Bookings(Id) ON DELETE SET NULL
        );

        CREATE TABLE IF NOT EXISTS Settings (
            SettingKey TEXT PRIMARY KEY,
            Value TEXT NOT NULL
        );
        """;
        command.ExecuteNonQuery();
    }

    private static void SeedInitialData(NpgsqlConnection connection)
    {
        using var transaction = connection.BeginTransaction();

        Execute(connection, transaction,
            "INSERT INTO Settings(SettingKey, Value) VALUES ('TariffPerHour', '150') ON CONFLICT(SettingKey) DO NOTHING");

        for (var number = 1; number <= 12; number++)
        {
            Execute(connection, transaction,
                "INSERT INTO Computers(Number, Name, IsActive) VALUES (@number, @name, TRUE) ON CONFLICT(Number) DO NOTHING",
                ("@number", number),
                ("@name", $"Компьютер №{number}"));
        }

        if (CountRows(connection, transaction, "Users") == 0)
        {
            Execute(connection, transaction,
                "INSERT INTO Users(Username, Password, Role) VALUES ('admin', 'admin123', 'admin')");
            Execute(connection, transaction,
                "INSERT INTO Users(Username, Password, Role) VALUES ('manager', 'manager123', 'manager')");
        }

        transaction.Commit();
    }

    private static int CountRows(NpgsqlConnection connection, NpgsqlTransaction transaction, string tableName)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"SELECT COUNT(*) FROM {tableName}";
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static void Execute(NpgsqlConnection connection, NpgsqlTransaction transaction, string sql, params (string Name, object? Value)[] parameters)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;

        foreach (var parameter in parameters)
            command.Parameters.AddWithValue(parameter.Name, parameter.Value ?? DBNull.Value);

        command.ExecuteNonQuery();
    }
}
