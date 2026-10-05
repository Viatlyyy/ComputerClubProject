using System;
using System.Collections.Generic;
using System.Globalization;
using Npgsql;
using ComputerClubWinForms.Data;
using ComputerClubWinForms.Models;

namespace ComputerClubWinForms.Managers;

public class SessionManager
{
    private readonly DatabaseHelper _db;
    private readonly ClientManager _clientManager;
    private readonly TariffManager _tariffManager;

    public string LastMessage { get; private set; } = string.Empty;

    public SessionManager(DatabaseHelper db, ClientManager clientManager, TariffManager tariffManager)
    {
        _db = db;
        _clientManager = clientManager;
        _tariffManager = tariffManager;
    }

    public List<Computer> GetComputers()
    {
        using var connection = _db.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Number, Name, IsActive FROM Computers WHERE IsActive = TRUE ORDER BY Number";

        var computers = new List<Computer>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            computers.Add(new Computer
            {
                Number = reader.GetInt32(0),
                Name = reader.GetString(1),
                IsActive = reader.GetBoolean(2)
            });
        }

        return computers;
    }

    public bool IsTimeSlotAvailable(DateTime start, TimeSpan duration, int computerNumber)
    {
        using var connection = _db.CreateConnection();
        return IsTimeSlotAvailable(connection, null, start, duration, computerNumber, null, null);
    }

    public bool CreateBooking(int clientId, int computerNumber, DateTime start, TimeSpan duration)
    {
        if (clientId <= 0) return SetMessage(false, "Клиент не выбран");
        if (computerNumber <= 0) return SetMessage(false, "Компьютер не выбран");
        if (duration.TotalMinutes <= 0) return SetMessage(false, "Длительность должна быть больше 0");
        if (start <= DateTime.Now) return SetMessage(false, "Нельзя забронировать прошедшее время");

        try
        {
            using var connection = _db.CreateConnection();
            if (_clientManager.GetClientById(connection, null, clientId) is null)
                return SetMessage(false, "Клиент не найден");
            if (!ComputerExists(connection, null, computerNumber))
                return SetMessage(false, "Компьютер не найден");
            if (!IsTimeSlotAvailable(connection, null, start, duration, computerNumber, null, null))
                return SetMessage(false, $"Компьютер №{computerNumber} уже забронирован на это время. Выберите другое время или компьютер");

            using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO Bookings(ClientId, ComputerNumber, PlannedStart, DurationMinutes, Status, CreatedAt)
                VALUES (@clientId, @computer, @start, @duration, 'active', @createdAt)
                """;
            command.Parameters.AddWithValue("@clientId", clientId);
            command.Parameters.AddWithValue("@computer", computerNumber);
            command.Parameters.AddWithValue("@start", ToDbDate(start));
            command.Parameters.AddWithValue("@duration", ToMinutes(duration));
            command.Parameters.AddWithValue("@createdAt", ToDbDate(DateTime.Now));
            command.ExecuteNonQuery();

            return SetMessage(true, "Бронь создана");
        }
        catch (Exception ex)
        {
            return SetMessage(false, "Ошибка базы данных. Бронь не сохранена. " + ex.Message);
        }
    }

    public bool CancelBooking(int bookingId)
    {
        try
        {
            using var connection = _db.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Bookings WHERE Id = @id AND Status = 'active'";
            command.Parameters.AddWithValue("@id", bookingId);
            var affected = command.ExecuteNonQuery();
            return SetMessage(affected > 0, affected > 0 ? "Бронь удалена" : "Бронь не найдена");
        }
        catch (Exception ex)
        {
            return SetMessage(false, "Ошибка базы данных: " + ex.Message);
        }
    }

    public Session? StartSession(int bookingId)
    {
        try
        {
            using var connection = _db.CreateConnection();
            using var transaction = connection.BeginTransaction();
            var booking = GetBookingById(connection, transaction, bookingId);

            if (booking is null || booking.Status != "active")
            {
                LastMessage = "Бронь не найдена";
                return null;
            }
            if (ClientHasActiveSession(connection, transaction, booking.ClientId))
            {
                LastMessage = "Клиент уже на сеансе. Сначала завершите текущий";
                return null;
            }

            var start = DateTime.Now;
            if (!IsTimeSlotAvailable(connection, transaction, start, booking.Duration, booking.ComputerNumber, booking.Id, null))
            {
                LastMessage = $"Компьютер №{booking.ComputerNumber} сейчас занят";
                return null;
            }

            var session = InsertActiveSession(connection, transaction, booking.ClientId, booking.ComputerNumber, start, booking.Duration, booking.Id);

            using var deleteBooking = connection.CreateCommand();
            deleteBooking.Transaction = transaction;
            deleteBooking.CommandText = "DELETE FROM Bookings WHERE Id = @id";
            deleteBooking.Parameters.AddWithValue("@id", booking.Id);
            deleteBooking.ExecuteNonQuery();

            transaction.Commit();
            LastMessage = "Сеанс открыт по брони";
            return session;
        }
        catch (Exception ex)
        {
            LastMessage = "Ошибка базы данных: " + ex.Message;
            return null;
        }
    }

    public Session? StartSessionDirect(int clientId, int computerNumber, DateTime start, TimeSpan duration)
    {
        if (duration.TotalMinutes <= 0)
        {
            LastMessage = "Укажите корректную длительность";
            return null;
        }

        try
        {
            using var connection = _db.CreateConnection();
            using var transaction = connection.BeginTransaction();

            if (_clientManager.GetClientById(connection, transaction, clientId) is null)
            {
                LastMessage = "Клиент не найден";
                return null;
            }
            if (!ComputerExists(connection, transaction, computerNumber))
            {
                LastMessage = "Компьютер не найден";
                return null;
            }
            if (ClientHasActiveSession(connection, transaction, clientId))
            {
                LastMessage = "Клиент уже на сеансе. Сначала завершите текущий";
                return null;
            }
            if (!IsTimeSlotAvailable(connection, transaction, start, duration, computerNumber, null, null))
            {
                LastMessage = $"Компьютер №{computerNumber} занят на выбранное время";
                return null;
            }

            var session = InsertActiveSession(connection, transaction, clientId, computerNumber, start, duration, null);
            transaction.Commit();
            LastMessage = "Сеанс открыт";
            return session;
        }
        catch (Exception ex)
        {
            LastMessage = "Ошибка базы данных: " + ex.Message;
            return null;
        }
    }

    public bool ExtendSession(int sessionId, TimeSpan additionalDuration)
    {
        if (additionalDuration.TotalMinutes <= 0)
            return SetMessage(false, "Введите положительное число");

        try
        {
            using var connection = _db.CreateConnection();
            using var transaction = connection.BeginTransaction();
            var session = GetSessionById(connection, transaction, sessionId);

            if (session is null) return SetMessage(false, "Сеанс не найден");
            if (session.IsCompleted) return SetMessage(false, "Сеанс уже завершён");

            var newPlannedEnd = session.PlannedEndTime.Add(additionalDuration);
            if (!IsTimeSlotAvailable(connection, transaction, session.StartTime, newPlannedEnd - session.StartTime, session.ComputerNumber, null, session.Id))
                return SetMessage(false, $"Невозможно продлить: компьютер №{session.ComputerNumber} забронирован на это время");

            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "UPDATE Sessions SET PlannedEndTime = @plannedEnd WHERE Id = @id AND IsCompleted = FALSE";
            command.Parameters.AddWithValue("@plannedEnd", ToDbDate(newPlannedEnd));
            command.Parameters.AddWithValue("@id", sessionId);
            command.ExecuteNonQuery();

            transaction.Commit();
            return SetMessage(true, "Сеанс продлён");
        }
        catch (Exception ex)
        {
            return SetMessage(false, "Ошибка базы данных: " + ex.Message);
        }
    }

    public Session? PreviewCompletion(int sessionId)
    {
        try
        {
            using var connection = _db.CreateConnection();
            var session = GetSessionById(connection, null, sessionId);
            if (session is null)
            {
                LastMessage = "Сеанс не найден";
                return null;
            }
            if (session.IsCompleted)
            {
                LastMessage = "Сеанс уже завершён";
                return null;
            }

            return PrepareCompletedSession(session, DateTime.Now);
        }
        catch (Exception ex)
        {
            LastMessage = "Ошибка базы данных: " + ex.Message;
            return null;
        }
    }

    public Session? CompleteSession(int sessionId)
    {
        try
        {
            using var connection = _db.CreateConnection();
            using var transaction = connection.BeginTransaction();
            var session = GetSessionById(connection, transaction, sessionId);

            if (session is null)
            {
                LastMessage = "Сеанс не найден";
                return null;
            }
            if (session.IsCompleted)
            {
                LastMessage = "Сеанс уже завершён";
                return null;
            }

            var completedSession = PrepareCompletedSession(session, DateTime.Now);
            var durationMinutes = ToMinutes(completedSession.Duration);
            var hours = durationMinutes / 60d;
            var clientBefore = _clientManager.GetClientById(connection, transaction, session.ClientId);
            var newDiscount = _tariffManager.GetPersonalDiscount((clientBefore?.TotalHours ?? 0d) + hours);

            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                UPDATE Sessions
                SET EndTime = @endTime,
                    DurationMinutes = @durationMinutes,
                    TotalCost = @totalCost,
                    TariffPerHour = @tariff,
                    OneTimeDiscountPercent = @oneTimeDiscount,
                    PersonalDiscountPercent = @personalDiscount,
                    IsCompleted = TRUE
                WHERE Id = @id AND IsCompleted = FALSE
                """;
            command.Parameters.AddWithValue("@endTime", ToDbDate(completedSession.EndTime!.Value));
            command.Parameters.AddWithValue("@durationMinutes", durationMinutes);
            command.Parameters.AddWithValue("@totalCost", completedSession.TotalCost);
            command.Parameters.AddWithValue("@tariff", completedSession.TariffPerHour);
            command.Parameters.AddWithValue("@oneTimeDiscount", completedSession.OneTimeDiscountPercent);
            command.Parameters.AddWithValue("@personalDiscount", completedSession.PersonalDiscountPercent);
            command.Parameters.AddWithValue("@id", sessionId);
            command.ExecuteNonQuery();

            if (!_clientManager.UpdateClientStats(session.ClientId, hours, completedSession.TotalCost, newDiscount, connection, transaction))
            {
                transaction.Rollback();
                LastMessage = _clientManager.LastMessage;
                return null;
            }

            transaction.Commit();
            LastMessage = "Сеанс закрыт, выручка учтена";
            return completedSession;
        }
        catch (Exception ex)
        {
            LastMessage = "Ошибка базы данных: " + ex.Message;
            return null;
        }
    }

    public List<Session> GetActiveSessions()
    {
        using var connection = _db.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT s.Id, s.ClientId, c.FullName, s.ComputerNumber, s.StartTime, s.PlannedEndTime,
                   s.EndTime, s.DurationMinutes, s.TotalCost, s.TariffPerHour,
                   s.OneTimeDiscountPercent, s.PersonalDiscountPercent, s.IsCompleted
            FROM Sessions s
            JOIN Clients c ON c.Id = s.ClientId
            WHERE s.IsCompleted = FALSE
            ORDER BY s.PlannedEndTime
            """;

        var sessions = new List<Session>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
            sessions.Add(ReadSession(reader));

        return sessions;
    }

    public List<Booking> GetActiveBookings()
    {
        using var connection = _db.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT b.Id, b.ClientId, c.FullName, b.ComputerNumber, b.PlannedStart, b.DurationMinutes, b.Status
            FROM Bookings b
            JOIN Clients c ON c.Id = b.ClientId
            WHERE b.Status = 'active'
            ORDER BY b.PlannedStart
            """;

        var bookings = new List<Booking>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
            bookings.Add(ReadBooking(reader));

        return bookings;
    }

    public List<Session> GetCompletedSessionsBetweenDates(DateTime from, DateTime to)
    {
        using var connection = _db.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT s.Id, s.ClientId, c.FullName, s.ComputerNumber, s.StartTime, s.PlannedEndTime,
                   s.EndTime, s.DurationMinutes, s.TotalCost, s.TariffPerHour,
                   s.OneTimeDiscountPercent, s.PersonalDiscountPercent, s.IsCompleted
            FROM Sessions s
            JOIN Clients c ON c.Id = s.ClientId
            WHERE s.IsCompleted = TRUE AND s.EndTime >= @from AND s.EndTime <= @to
            ORDER BY s.EndTime
            """;
        command.Parameters.AddWithValue("@from", ToDbDate(from));
        command.Parameters.AddWithValue("@to", ToDbDate(to));

        var sessions = new List<Session>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
            sessions.Add(ReadSession(reader));

        return sessions;
    }

    private Session PrepareCompletedSession(Session session, DateTime endTime)
    {
        if (endTime < session.StartTime)
            endTime = session.StartTime;

        var duration = endTime - session.StartTime;
        if (duration.TotalMinutes < 1)
            duration = TimeSpan.FromMinutes(1);

        session.EndTime = endTime;
        session.Duration = duration;
        session.IsCompleted = true;
        _tariffManager.CalculateSessionCost(session);
        return session;
    }

    private Session InsertActiveSession(NpgsqlConnection connection, NpgsqlTransaction transaction, int clientId, int computerNumber, DateTime start, TimeSpan duration, int? bookingId)
    {
        var plannedEnd = start.Add(duration);
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO Sessions(ClientId, ComputerNumber, StartTime, PlannedEndTime, EndTime, DurationMinutes,
                                 TotalCost, TariffPerHour, OneTimeDiscountPercent, PersonalDiscountPercent, IsCompleted, BookingId)
            VALUES (@clientId, @computer, @start, @plannedEnd, NULL, 0, 0, 0, 0, 0, FALSE, @bookingId)
            RETURNING Id
            """;
        command.Parameters.AddWithValue("@clientId", clientId);
        command.Parameters.AddWithValue("@computer", computerNumber);
        command.Parameters.AddWithValue("@start", ToDbDate(start));
        command.Parameters.AddWithValue("@plannedEnd", ToDbDate(plannedEnd));
        command.Parameters.AddWithValue("@bookingId", bookingId.HasValue ? (object)bookingId.Value : DBNull.Value);

        var id = Convert.ToInt32(command.ExecuteScalar());
        return new Session
        {
            Id = id,
            ClientId = clientId,
            ComputerNumber = computerNumber,
            StartTime = start,
            PlannedEndTime = plannedEnd,
            IsCompleted = false
        };
    }

    private bool IsTimeSlotAvailable(NpgsqlConnection connection, NpgsqlTransaction? transaction, DateTime start, TimeSpan duration, int computerNumber, int? ignoreBookingId, int? ignoreSessionId)
    {
        if (duration.TotalMinutes <= 0)
            return false;

        var end = start.Add(duration);

        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = "SELECT Id, PlannedStart, DurationMinutes FROM Bookings WHERE Status = 'active' AND ComputerNumber = @computer";
            command.Parameters.AddWithValue("@computer", computerNumber);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var id = reader.GetInt32(0);
                if (ignoreBookingId.HasValue && ignoreBookingId.Value == id)
                    continue;

                var oldStart = ReadDate(reader, 1);
                var oldEnd = oldStart.Add(TimeSpan.FromMinutes(reader.GetInt32(2)));
                if (IntervalsOverlap(start, end, oldStart, oldEnd))
                    return false;
            }
        }

        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = "SELECT Id, StartTime, PlannedEndTime FROM Sessions WHERE IsCompleted = FALSE AND ComputerNumber = @computer";
            command.Parameters.AddWithValue("@computer", computerNumber);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var id = reader.GetInt32(0);
                if (ignoreSessionId.HasValue && ignoreSessionId.Value == id)
                    continue;

                var oldStart = ReadDate(reader, 1);
                var oldEnd = ReadDate(reader, 2);
                if (IntervalsOverlap(start, end, oldStart, oldEnd))
                    return false;
            }
        }

        return true;
    }

    private bool SetMessage(bool result, string message)
    {
        LastMessage = message;
        return result;
    }

    private static bool IntervalsOverlap(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
    {
        return start1 < end2 && start2 < end1;
    }

    private static bool ClientHasActiveSession(NpgsqlConnection connection, NpgsqlTransaction? transaction, int clientId)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT COUNT(*) FROM Sessions WHERE ClientId = @clientId AND IsCompleted = FALSE";
        command.Parameters.AddWithValue("@clientId", clientId);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private static bool ComputerExists(NpgsqlConnection connection, NpgsqlTransaction? transaction, int computerNumber)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT COUNT(*) FROM Computers WHERE Number = @number AND IsActive = TRUE";
        command.Parameters.AddWithValue("@number", computerNumber);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private Booking? GetBookingById(NpgsqlConnection connection, NpgsqlTransaction? transaction, int bookingId)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT b.Id, b.ClientId, c.FullName, b.ComputerNumber, b.PlannedStart, b.DurationMinutes, b.Status
            FROM Bookings b
            JOIN Clients c ON c.Id = b.ClientId
            WHERE b.Id = @id
            """;
        command.Parameters.AddWithValue("@id", bookingId);

        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadBooking(reader) : null;
    }

    private Session? GetSessionById(NpgsqlConnection connection, NpgsqlTransaction? transaction, int sessionId)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT s.Id, s.ClientId, c.FullName, s.ComputerNumber, s.StartTime, s.PlannedEndTime,
                   s.EndTime, s.DurationMinutes, s.TotalCost, s.TariffPerHour,
                   s.OneTimeDiscountPercent, s.PersonalDiscountPercent, s.IsCompleted
            FROM Sessions s
            JOIN Clients c ON c.Id = s.ClientId
            WHERE s.Id = @id
            """;
        command.Parameters.AddWithValue("@id", sessionId);

        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadSession(reader) : null;
    }

    private static Booking ReadBooking(NpgsqlDataReader reader)
    {
        return new Booking
        {
            Id = reader.GetInt32(0),
            ClientId = reader.GetInt32(1),
            ClientName = reader.GetString(2),
            ComputerNumber = reader.GetInt32(3),
            PlannedStart = ReadDate(reader, 4),
            Duration = TimeSpan.FromMinutes(reader.GetInt32(5)),
            Status = reader.GetString(6)
        };
    }

    private static Session ReadSession(NpgsqlDataReader reader)
    {
        return new Session
        {
            Id = reader.GetInt32(0),
            ClientId = reader.GetInt32(1),
            ClientName = reader.GetString(2),
            ComputerNumber = reader.GetInt32(3),
            StartTime = ReadDate(reader, 4),
            PlannedEndTime = ReadDate(reader, 5),
            EndTime = reader.IsDBNull(6) ? null : ReadDate(reader, 6),
            Duration = TimeSpan.FromMinutes(Convert.ToInt32(reader.GetValue(7), CultureInfo.InvariantCulture)),
            TotalCost = Convert.ToDecimal(reader.GetValue(8), CultureInfo.InvariantCulture),
            TariffPerHour = Convert.ToDecimal(reader.GetValue(9), CultureInfo.InvariantCulture),
            OneTimeDiscountPercent = Convert.ToDecimal(reader.GetValue(10), CultureInfo.InvariantCulture),
            PersonalDiscountPercent = Convert.ToDecimal(reader.GetValue(11), CultureInfo.InvariantCulture),
            IsCompleted = reader.GetBoolean(12)
        };
    }

    private static int ToMinutes(TimeSpan duration)
    {
        return Math.Max(1, (int)Math.Ceiling(duration.TotalMinutes));
    }

    private static DateTime ToDbDate(DateTime value)
    {
        return DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
    }

    private static DateTime ReadDate(NpgsqlDataReader reader, int ordinal)
    {
        return DateTime.SpecifyKind(reader.GetDateTime(ordinal), DateTimeKind.Unspecified);
    }
}
