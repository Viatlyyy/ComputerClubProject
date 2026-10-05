using System;
using System.Collections.Generic;
using System.Globalization;
using Npgsql;
using ComputerClubWinForms.Data;
using ComputerClubWinForms.Models;

namespace ComputerClubWinForms.Managers;

public class ClientManager
{
    private readonly DatabaseHelper _db;

    public string LastMessage { get; private set; } = string.Empty;

    public ClientManager(DatabaseHelper db)
    {
        _db = db;
    }

    public bool AddClient(Client client)
    {
        if (!ValidateClient(client))
            return false;

        try
        {
            using var connection = _db.CreateConnection();
            if (PhoneExists(connection, client.Phone, null))
                return Fail("Клиент с таким телефоном уже зарегистрирован");

            using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO Clients(FullName, Phone, TotalHours, TotalSpent, DiscountPercent)
                VALUES (@fullName, @phone, 0, 0, 0)
                """;
            command.Parameters.AddWithValue("@fullName", client.FullName.Trim());
            command.Parameters.AddWithValue("@phone", client.Phone.Trim());
            command.ExecuteNonQuery();

            return Ok("Клиент добавлен");
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return Fail("Клиент с таким телефоном уже зарегистрирован");
        }
        catch (Exception ex)
        {
            return Fail($"Ошибка базы данных: {ex.Message}");
        }
    }

    public bool EditClient(Client client)
    {
        if (!ValidateClient(client))
            return false;

        try
        {
            using var connection = _db.CreateConnection();
            if (PhoneExists(connection, client.Phone, client.Id))
                return Fail("Телефон уже занят другим клиентом");

            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE Clients SET FullName = @fullName, Phone = @phone WHERE Id = @id";
            command.Parameters.AddWithValue("@fullName", client.FullName.Trim());
            command.Parameters.AddWithValue("@phone", client.Phone.Trim());
            command.Parameters.AddWithValue("@id", client.Id);

            var affected = command.ExecuteNonQuery();
            return affected > 0 ? Ok("Клиент обновлён") : Fail("Клиент не найден");
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return Fail("Телефон уже занят другим клиентом");
        }
        catch (Exception ex)
        {
            return Fail($"Ошибка базы данных: {ex.Message}");
        }
    }

    public bool DeleteClient(int clientId)
    {
        try
        {
            using var connection = _db.CreateConnection();
            if (HasActiveSession(connection, clientId))
                return Fail("Невозможно удалить клиента: у него есть активный сеанс");

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Clients WHERE Id = @id";
            command.Parameters.AddWithValue("@id", clientId);

            var affected = command.ExecuteNonQuery();
            return affected > 0 ? Ok("Клиент удалён") : Fail("Клиент не найден");
        }
        catch (Exception ex)
        {
            return Fail($"Ошибка базы данных: {ex.Message}");
        }
    }

    public List<Client> GetAllClients(string? searchText = null)
    {
        using var connection = _db.CreateConnection();
        using var command = connection.CreateCommand();

        if (string.IsNullOrWhiteSpace(searchText))
        {
            command.CommandText = "SELECT Id, FullName, Phone, TotalHours, TotalSpent, DiscountPercent FROM Clients ORDER BY FullName";
        }
        else
        {
            command.CommandText = """
                SELECT Id, FullName, Phone, TotalHours, TotalSpent, DiscountPercent
                FROM Clients
                WHERE FullName LIKE @term OR Phone LIKE @term
                ORDER BY FullName
                """;
            command.Parameters.AddWithValue("@term", $"%{searchText.Trim()}%");
        }

        var clients = new List<Client>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
            clients.Add(ReadClient(reader));

        return clients;
    }

    public Client? GetClientById(int clientId)
    {
        using var connection = _db.CreateConnection();
        return GetClientById(connection, null, clientId);
    }

    public Client? GetClientById(NpgsqlConnection connection, NpgsqlTransaction? transaction, int clientId)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT Id, FullName, Phone, TotalHours, TotalSpent, DiscountPercent FROM Clients WHERE Id = @id";
        command.Parameters.AddWithValue("@id", clientId);

        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadClient(reader) : null;
    }

    public double GetClientTotalHours(int clientId)
    {
        using var connection = _db.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT TotalHours FROM Clients WHERE Id = @id";
        command.Parameters.AddWithValue("@id", clientId);
        var value = command.ExecuteScalar();
        return value is null || value is DBNull ? 0d : Convert.ToDouble(value, CultureInfo.InvariantCulture);
    }

    public bool UpdateClientStats(
        int clientId,
        double hours,
        decimal spent,
        int discount,
        NpgsqlConnection connection,
        NpgsqlTransaction transaction)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            UPDATE Clients
            SET TotalHours = TotalHours + @hours,
                TotalSpent = TotalSpent + @spent,
                DiscountPercent = @discount
            WHERE Id = @clientId
            """;
        command.Parameters.AddWithValue("@hours", hours);
        command.Parameters.AddWithValue("@spent", spent);
        command.Parameters.AddWithValue("@discount", discount);
        command.Parameters.AddWithValue("@clientId", clientId);

        return command.ExecuteNonQuery() > 0
            ? Ok("Статистика клиента обновлена")
            : Fail("Клиент не найден");
    }

    private static Client ReadClient(NpgsqlDataReader reader)
    {
        return new Client
        {
            Id = reader.GetInt32(0),
            FullName = reader.GetString(1),
            Phone = reader.GetString(2),
            TotalHours = Convert.ToDouble(reader.GetValue(3), CultureInfo.InvariantCulture),
            TotalSpent = Convert.ToDecimal(reader.GetValue(4), CultureInfo.InvariantCulture),
            DiscountPercent = Convert.ToInt32(reader.GetValue(5), CultureInfo.InvariantCulture)
        };
    }

    private bool ValidateClient(Client? client)
    {
        if (client is null)
            return Fail("Данные клиента не переданы");
        if (string.IsNullOrWhiteSpace(client.FullName))
            return Fail("Заполните ФИО клиента");
        if (string.IsNullOrWhiteSpace(client.Phone))
            return Fail("Заполните телефон клиента");
        return true;
    }

    private static bool PhoneExists(NpgsqlConnection connection, string phone, int? excludeClientId)
    {
        using var command = connection.CreateCommand();
        if (excludeClientId.HasValue)
        {
            command.CommandText = "SELECT COUNT(*) FROM Clients WHERE Phone = @phone AND Id <> @id";
            command.Parameters.AddWithValue("@id", excludeClientId.Value);
        }
        else
        {
            command.CommandText = "SELECT COUNT(*) FROM Clients WHERE Phone = @phone";
        }

        command.Parameters.AddWithValue("@phone", phone.Trim());
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private static bool HasActiveSession(NpgsqlConnection connection, int clientId)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Sessions WHERE ClientId = @clientId AND IsCompleted = FALSE";
        command.Parameters.AddWithValue("@clientId", clientId);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private bool Ok(string message)
    {
        LastMessage = message;
        return true;
    }

    private bool Fail(string message)
    {
        LastMessage = message;
        return false;
    }
}
