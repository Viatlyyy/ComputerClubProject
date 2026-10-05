using System;
using System.Windows.Forms;
using ComputerClubWinForms.Data;
using ComputerClubWinForms.Managers;
using ComputerClubWinForms.Models;

namespace ComputerClubWinForms.Forms;

public partial class MainForm : Form
{
    private DatabaseHelper? _db;

    public bool IsLogoutRequested { get; private set; }

    public MainForm()
    {
        InitializeComponent();
    }

    public MainForm(DatabaseHelper db, User currentUser) : this()
    {
        _db = db;
        InitializeAfterLogin(currentUser);
    }

    public void InitializeAfterLogin(User user)
    {
        if (_db is null)
            return;

        _currentUserLabel.Text = $"Пользователь: {user.Username} | Роль: {user.Role}";
        _mainTabControl.TabPages.Clear();

        var clientManager = new ClientManager(_db);
        var tariffManager = new TariffManager(_db, clientManager);
        var sessionManager = new SessionManager(_db, clientManager, tariffManager);
        var analyticsManager = new AnalyticsManager(sessionManager);

        var clientPage = new ClientPage(clientManager);
        var sessionPage = new SessionPage(clientManager, sessionManager);

        clientPage.ClientsChanged += (_, _) => sessionPage.ReloadClients();

        _mainTabControl.TabPages.Add(CreateRuntimeTab("Клиенты", clientPage));
        _mainTabControl.TabPages.Add(CreateRuntimeTab("Сеансы", sessionPage));

        if (user.IsAdmin)
        {
            _mainTabControl.TabPages.Add(CreateRuntimeTab("Отчёты", new ReportPage(analyticsManager)));
            _mainTabControl.TabPages.Add(CreateRuntimeTab("Настройки", new SettingsPage(tariffManager)));
        }
    }

    public void SwitchToPage(int index)
    {
        if (index >= 0 && index < _mainTabControl.TabPages.Count)
            _mainTabControl.SelectedIndex = index;
    }

    private static TabPage CreateRuntimeTab(string title, Control content)
    {
        content.Dock = DockStyle.Fill;
        var tab = new TabPage(title);
        tab.Controls.Add(content);
        return tab;
    }

    private void OnLogoutClick(object? sender, EventArgs e)
    {
        IsLogoutRequested = true;
        Close();
    }
}
