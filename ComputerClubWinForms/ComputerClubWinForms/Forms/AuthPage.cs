using System;
using System.Windows.Forms;
using ComputerClubWinForms.Data;
using ComputerClubWinForms.Managers;
using ComputerClubWinForms.Models;

namespace ComputerClubWinForms.Forms;

public partial class AuthPage : Form
{
    private DatabaseHelper? _db;
    private UserManager? _userManager;

    public AuthPage()
    {
        InitializeComponent();
    }

    public AuthPage(DatabaseHelper db) : this()
    {
        _db = db;
        _userManager = new UserManager(db);
    }

    private void OnLoginClick(object? sender, EventArgs e)
    {
        _errorLabel.Text = string.Empty;

        if (_userManager is null || _db is null)
        {
            ShowError("Форма открыта в конструкторе Visual Studio. Запустите приложение для входа в систему.");
            return;
        }

        var user = _userManager.Login(_usernameTextBox.Text, _passwordTextBox.Text);
        if (user is null)
        {
            ShowError(_userManager.LastMessage);
            return;
        }

        OpenMainForm(user);
    }

    private void OpenMainForm(User user)
    {
        Hide();

        var mainForm = new MainForm(_db!, user);
        mainForm.FormClosed += (_, _) =>
        {
            if (mainForm.IsLogoutRequested)
            {
                _passwordTextBox.Clear();
                _usernameTextBox.Focus();
                Show();
            }
            else
            {
                Close();
            }
        };
        mainForm.Show();
    }

    private void ShowError(string message)
    {
        _errorLabel.Text = message;
    }
}
