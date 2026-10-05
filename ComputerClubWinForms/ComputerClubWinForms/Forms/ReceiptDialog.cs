using System;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using ComputerClubWinForms.Managers;
using ComputerClubWinForms.Models;

namespace ComputerClubWinForms.Forms;

public partial class ReceiptDialog : Form
{
    private Session _session = CreateDesignSession();
    private bool _confirmMode;

    public ReceiptDialog()
    {
        InitializeComponent();
        _textBox.Text = BuildText(_session);
    }

    public ReceiptDialog(Session session, bool confirmMode) : this()
    {
        _session = session;
        _confirmMode = confirmMode;
        Text = _confirmMode ? "Подтверждение оплаты" : "Чек закрытого сеанса";
        _btnOk.Text = _confirmMode ? "Подтвердить оплату" : "ОК";
        _btnCancel.Visible = _confirmMode;
        CancelButton = _confirmMode ? _btnCancel : _btnOk;
        _textBox.Text = BuildText(session);
    }

    private static Session CreateDesignSession()
    {
        return new Session
        {
            Id = 1,
            ClientName = "Иванов Иван",
            ComputerNumber = 3,
            StartTime = DateTime.Today.AddHours(12),
            EndTime = DateTime.Today.AddHours(14).AddMinutes(30),
            Duration = TimeSpan.FromMinutes(150),
            TariffPerHour = 150m,
            OneTimeDiscountPercent = 0m,
            PersonalDiscountPercent = 5m,
            TotalCost = 356.25m
        };
    }

    private static string BuildText(Session session)
    {
        var culture = CultureInfo.GetCultureInfo("ru-RU");
        var endTime = session.EndTime ?? DateTime.Now;
        var billableHours = TariffManager.GetBillableHours(session.Duration);
        var baseCost = Math.Round((decimal)billableHours * session.TariffPerHour, 2);
        var totalDiscount = session.OneTimeDiscountPercent + session.PersonalDiscountPercent;

        var sb = new StringBuilder();
        sb.AppendLine("КОМПЬЮТЕРНЫЙ КЛУБ");
        sb.AppendLine("Детализация сеанса");
        sb.AppendLine(new string('-', 42));
        sb.AppendLine($"Клиент:          {session.ClientName}");
        sb.AppendLine($"Компьютер:       №{session.ComputerNumber}");
        sb.AppendLine($"Начало:          {session.StartTime:dd.MM.yyyy HH:mm:ss}");
        sb.AppendLine($"Окончание:       {endTime:dd.MM.yyyy HH:mm:ss}");
        sb.AppendLine($"Длительность:    {FormatDuration(session.Duration)}");
        sb.AppendLine(new string('-', 42));
        sb.AppendLine($"Тариф:           {session.TariffPerHour.ToString("N2", culture)} руб./час");
        sb.AppendLine($"К оплате часов:  {billableHours.ToString("N2", culture)}");
        sb.AppendLine($"Базовая сумма:   {baseCost.ToString("N2", culture)} руб.");
        sb.AppendLine($"Разовая скидка:  {session.OneTimeDiscountPercent.ToString("N0", culture)}%");
        sb.AppendLine($"Накоп. скидка:   {session.PersonalDiscountPercent.ToString("N0", culture)}%");
        sb.AppendLine($"Общая скидка:    {totalDiscount.ToString("N0", culture)}%");
        sb.AppendLine(new string('-', 42));
        sb.AppendLine($"ИТОГО:           {session.TotalCost.ToString("N2", culture)} руб.");
        return sb.ToString();
    }

    private static string FormatDuration(TimeSpan value)
    {
        return $"{(int)value.TotalHours} ч {value.Minutes} мин";
    }
}
