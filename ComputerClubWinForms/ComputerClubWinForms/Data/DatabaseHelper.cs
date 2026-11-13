using System;
using Npgsql;

namespace ComputerClubWinForms.Data;

public class DatabaseHelper
{
    public string ConnectionString { get; }

    public DatabaseHelper() : this(BuildConnectionString())
    {
    }

    public DatabaseHelper(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Строка подключения не должна быть пустой.", nameof(connectionString));

        ConnectionString = connectionString;
    }

    private static string BuildConnectionString()
    {
        var settings = new NpgsqlConnectionStringBuilder
        {
            Host = Environment.GetEnvironmentVariable("PGHOST") ?? "localhost",
            Port = int.TryParse(Environment.GetEnvironmentVariable("PGPORT"), out var port) && port is > 0 and <= 65535 ? port : 5432,
            Database = Environment.GetEnvironmentVariable("PGDATABASE") ?? "computer_club",
            Username = Environment.GetEnvironmentVariable("PGUSER") ?? "postgres"
        };
        var password = Environment.GetEnvironmentVariable("PGPASSWORD");
        if (!string.IsNullOrEmpty(password))
            settings.Password = password;
        return settings.ConnectionString;
    }

    public NpgsqlConnection CreateConnection()
    {
        var connection = new NpgsqlConnection(ConnectionString);
        try
        {
            connection.Open();
            return connection;
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }
}
