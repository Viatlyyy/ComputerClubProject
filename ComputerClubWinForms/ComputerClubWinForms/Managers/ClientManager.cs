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
            var phone = GetStoredPhone(client.Phone);
            using var connection = _db.CreateConnection();
            if (PhoneExists(connection, phone, null))
                return Fail("Клиент с таким телефоном уже зарегистрирован");

            using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO Clients(FullName, Phone, TotalHours, TotalSpent, DiscountPercent)
                VALUES (@fullName, @phone, 0, 0, 0)
                RETURNING Id
                """;
            command.Parameters.AddWithValue("@fullName", client.FullName.Trim());
            command.Parameters.AddWithValue("@phone", phone);
            client.Id = Convert.ToInt32(command.ExecuteScalar());
            client.Phone = phone;

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
            var phone = GetStoredPhone(client.Phone);
            using var connection = _db.CreateConnection();
            if (PhoneExists(connection, phone, client.Id))
                return Fail("Телефон уже занят другим клиентом");

            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE Clients SET FullName = @fullName, Phone = @phone WHERE Id = @id";
            command.Parameters.AddWithValue("@fullName", client.FullName.Trim());
            command.Parameters.AddWithValue("@phone", phone);
            command.Parameters.AddWithValue("@id", client.Id);

            var affected = command.ExecuteNonQuery();
            if (affected == 0)
                return Fail("Клиент не найден");
            client.Phone = phone;
            return Ok("Клиент обновлён");
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
            command.CommandText = "SELECT COUNT(*) FROM Sessions WHERE ClientId = @id";
            command.Parameters.AddWithValue("@id", clientId);
            if (Convert.ToInt64(command.ExecuteScalar()) > 0)
                return Fail("Невозможно удалить клиента: сохранена история сеансов и оплат");
            command.Parameters.Clear();
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
                WHERE FullName ILIKE @term OR Phone ILIKE @term
                   OR (@phonePattern <> '' AND regexp_replace(Phone, '[^0-9]', '', 'g') ~ @phonePattern)
                ORDER BY FullName
                """;
            var search = searchText.Trim();
            command.Parameters.AddWithValue("@term", $"%{search}%");
            command.Parameters.AddWithValue("@phonePattern", BuildPhoneSearchPattern(search));
        }

        var clients = new List<Client>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
            clients.Add(ReadClient(reader));

        return clients;
    }

    internal static string BuildPhoneSearchPattern(string search)
    {
        var digits = Client.GetPhoneDigits(search);
        if (digits.Length > 15)
            return string.Empty;

        if (digits.Length is >= 2 and <= 11 && (digits[0] is '7' or '8'))
        {
            var prefix = $"^[78]?{digits[1..]}[0-9]{{{11 - digits.Length}}}$";
            return digits.Length == 11 || search[0] == '+' ? prefix : $"{digits}|{prefix}";
        }

        return digits;
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

    private static string GetStoredPhone(string phone) =>
        Client.TryGetRussianPhoneDigits(phone, out var localDigits) ? "7" + localDigits : phone.Trim();

    private static bool PhoneExists(NpgsqlConnection connection, string phone, int? excludeClientId)
    {
        using var command = connection.CreateCommand();
        Client.TryGetRussianPhoneDigits(phone, out var localDigits);
        command.CommandText = """
            SELECT EXISTS (
                SELECT 1 FROM Clients
                WHERE (@id = 0 OR Id <> @id)
                  AND (Phone = @phone OR (@digits <> '' AND
                      regexp_replace(Phone, '[^0-9]', '', 'g') IN (@digits, '7' || @digits, '8' || @digits)))
            )
            """;
        command.Parameters.AddWithValue("@id", excludeClientId.GetValueOrDefault());
        command.Parameters.AddWithValue("@phone", phone);
        command.Parameters.AddWithValue("@digits", localDigits);
        return (bool)command.ExecuteScalar()!;
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
