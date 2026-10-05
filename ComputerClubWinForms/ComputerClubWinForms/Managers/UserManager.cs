using System;
using ComputerClubWinForms.Data;
using ComputerClubWinForms.Models;

namespace ComputerClubWinForms.Managers;

public class UserManager
{
    private readonly DatabaseHelper _db;
    private User? _currentUser;

    public string LastMessage { get; private set; } = string.Empty;

    public UserManager(DatabaseHelper db)
    {
        _db = db;
    }

    public User? Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return Fail("Введите логин и пароль");

        try
        {
            using var connection = _db.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Username, Password, Role FROM Users WHERE Username = @username LIMIT 1";
            command.Parameters.AddWithValue("@username", username.Trim());

            using var reader = command.ExecuteReader();
            if (!reader.Read())
                return Fail("Неверное имя пользователя или пароль");

            var user = new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                Password = reader.GetString(2),
                Role = reader.GetString(3)
            };

            if (password != user.Password)
                return Fail("Неверное имя пользователя или пароль");

            _currentUser = user;
            LastMessage = "Вход выполнен";
            return user;
        }
        catch (Exception ex)
        {
            return Fail($"Ошибка подключения к базе данных: {ex.Message}");
        }
    }

    public User? GetCurrentUser()
    {
        return _currentUser;
    }

    public void Logout()
    {
        _currentUser = null;
        LastMessage = "Выход выполнен";
    }

    private User? Fail(string message)
    {
        LastMessage = message;
        return null;
    }
}
