using System;
using Npgsql;

namespace ComputerClubWinForms.Data;

public class DatabaseHelper
{
    private const string DefaultConnectionString =
        "Host=localhost;Username=postgres;Database=ComputerClub";

    public string ConnectionString { get; }

    public DatabaseHelper() : this(Environment.GetEnvironmentVariable("COMPUTER_CLUB_CONNECTION_STRING") ?? DefaultConnectionString)
    {
    }

    public DatabaseHelper(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Строка подключения не должна быть пустой.", nameof(connectionString));

        ConnectionString = connectionString;
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
