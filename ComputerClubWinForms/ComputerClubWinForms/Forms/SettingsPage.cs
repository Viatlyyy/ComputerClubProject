using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using ComputerClubWinForms.Managers;

namespace ComputerClubWinForms.Forms;

public partial class SettingsPage : UserControl
{
    private TariffManager? _tariffManager;

    public SettingsPage()
    {
        InitializeComponent();

        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            ShowCurrentTariff(150m);
    }

    public SettingsPage(TariffManager tariffManager) : this()
    {
        _tariffManager = tariffManager;
        ShowCurrentTariff(_tariffManager.GetCurrentTariff());
    }

    private void OnSaveTariffClick(object? sender, EventArgs e)
    {
        if (_tariffManager is null)
            return;

        if (!decimal.TryParse(_txtTariff.Text.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var tariff) || tariff <= 0m)
        {
            ShowMessage(false, "Цена должна быть положительным числом");
            return;
        }

        var success = _tariffManager.UpdateTariff(tariff);
        ShowMessage(success, _tariffManager.LastMessage);
        if (success)
        {
            ShowCurrentTariff(tariff);
            _txtTariff.Clear();
        }
    }

    public void ShowCurrentTariff(decimal tariff)
    {
        var culture = CultureInfo.GetCultureInfo("ru-RU");
        _lblCurrentTariff.Text = $"Текущий тариф: {tariff.ToString("N2", culture)} руб./час";
    }

    public void ShowMessage(bool success, string message)
    {
        MessageBox.Show(message, success ? "Готово" : "Ошибка", MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
    }
}
