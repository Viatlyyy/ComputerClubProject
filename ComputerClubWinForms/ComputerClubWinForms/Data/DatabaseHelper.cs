using System;
using Npgsql;

namespace ComputerClubWinForms.Data;

public class DatabaseHelper
{
    private const string DefaultConnectionString =
        "Host=192.168.1.48;Username=st50-4;Password=<ПАРОЛЬ>;Database=ComputerClub";

    public string ConnectionString { get; }

    public DatabaseHelper() : this(DefaultConnectionString)
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
