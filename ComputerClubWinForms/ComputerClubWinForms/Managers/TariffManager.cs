using System;
using System.Globalization;
using ComputerClubWinForms.Data;
using ComputerClubWinForms.Models;

namespace ComputerClubWinForms.Managers;

public class TariffManager
{
    private readonly DatabaseHelper _db;
    private readonly ClientManager _clientManager;

    public string LastMessage { get; private set; } = string.Empty;

    public TariffManager(DatabaseHelper db, ClientManager clientManager)
    {
        _db = db;
        _clientManager = clientManager;
    }

    public decimal GetCurrentTariff()
    {
        using var connection = _db.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Value FROM Settings WHERE SettingKey = 'TariffPerHour'";

        var value = command.ExecuteScalar()?.ToString();
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var tariff))
            return tariff;

        return 150m;
    }

    public bool UpdateTariff(decimal newTariff)
    {
        if (newTariff <= 0m)
            return Fail("Цена должна быть положительным числом");

        try
        {
            using var connection = _db.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO Settings(SettingKey, Value) VALUES ('TariffPerHour', @value)
                ON CONFLICT(SettingKey) DO UPDATE SET Value = excluded.Value
                """;
            command.Parameters.AddWithValue("@value", newTariff.ToString(CultureInfo.InvariantCulture));
            command.ExecuteNonQuery();

            return Ok("Тариф сохранён");
        }
        catch (Exception ex)
        {
            return Fail("Не удалось сохранить тариф: " + ex.Message);
        }
    }

    public void CalculateSessionCost(Session session)
    {
        var tariff = GetCurrentTariff();
        var hours = GetBillableHours(session.Duration);
        var baseCost = Math.Round((decimal)hours * tariff, 2);

        var oneTimeDiscount = session.Duration.TotalHours > 3 ? 5m : 0m;
        var personalDiscount = GetPersonalDiscount(_clientManager.GetClientTotalHours(session.ClientId));
        var totalDiscount = oneTimeDiscount + personalDiscount;
        var total = Math.Round(baseCost * (1m - totalDiscount / 100m), 2);

        session.TariffPerHour = tariff;
        session.OneTimeDiscountPercent = oneTimeDiscount;
        session.PersonalDiscountPercent = personalDiscount;
        session.TotalCost = total < 0 ? 0 : total;
    }


    public static double GetBillableHours(TimeSpan duration)
    {
        return Math.Max(duration.TotalMinutes, 1) / 60d;
    }

    public int GetPersonalDiscount(double totalHours)
    {
        if (totalHours < 10) return 0;
        if (totalHours < 20) return 5;
        if (totalHours < 30) return 10;
        return 15;
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
