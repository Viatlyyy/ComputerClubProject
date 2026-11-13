using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ComputerClubWinForms.Managers;
using ComputerClubWinForms.Models;

namespace ComputerClubWinForms.Forms;

public partial class BookingDialog : Form
{
    private SessionManager? _sessionManager;
    private int? _initialComputerNumber;
    private readonly Dictionary<int, bool> _computerAvailability = new();

    public int ClientId { get; private set; }
    public DateTime SelectedStart => _dtpDate.Value.Date + _dtpTime.Value.TimeOfDay;
    public TimeSpan SelectedDuration => TimeSpan.FromMinutes((double)_numDurationMinutes.Value);
    public int SelectedComputerNumber
    {
        get
        {
            if (_cmbComputers.SelectedValue is int number)
                return number;
            return int.TryParse(_cmbComputers.SelectedValue?.ToString(), out var parsed) ? parsed : 0;
        }
    }

    public BookingDialog()
    {
        InitializeComponent();
        _computerMap.SizeChanged += (_, _) => LayoutComputerMapButtons();
        SetDefaultDateTime();

        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
        {
            LoadDesignPreviewComputers();
            UpdateComputerMap();
        }
    }

    public BookingDialog(SessionManager sessionManager, int clientId, int? initialComputerNumber) : this()
    {
        _sessionManager = sessionManager;
        ClientId = clientId;
        _initialComputerNumber = initialComputerNumber;
        LoadComputers();
        UpdateComputerMap();
    }

    private void SetDefaultDateTime()
    {
        _dtpDate.Value = DateTime.Now.Date.AddDays(1);
        _dtpTime.Value = DateTime.Today.AddHours(Math.Min(DateTime.Now.Hour + 1, 23));
    }

    private void DateOrDurationChanged(object? sender, EventArgs e)
    {
        UpdateComputerMap();
    }

    private void ComputerSelectionChanged(object? sender, EventArgs e)
    {
        UpdateComputerMapSelection();
    }

    private void LoadDesignPreviewComputers()
    {
        var computers = Enumerable.Range(1, 10)
            .Select(number => new Computer { Number = number, Name = $"ПК {number}", IsActive = true })
            .ToList();

        _cmbComputers.DataSource = computers;
        _cmbComputers.DisplayMember = nameof(Computer.Number);
        _cmbComputers.ValueMember = nameof(Computer.Number);
    }

    private void LoadComputers()
    {
        if (_sessionManager is null)
            return;

        var computers = _sessionManager.GetComputers();
        _cmbComputers.DataSource = computers;
        _cmbComputers.DisplayMember = nameof(Computer.Number);
        _cmbComputers.ValueMember = nameof(Computer.Number);
        if (_initialComputerNumber.HasValue)
            _cmbComputers.SelectedValue = _initialComputerNumber.Value;
    }

    private void UpdateComputerMap()
    {
        _computerMap.SuspendLayout();
        _computerMap.Controls.Clear();
        _computerAvailability.Clear();

        var start = SelectedStart;
        var duration = SelectedDuration;
        var computers = _sessionManager?.GetComputers()
            ?? Enumerable.Range(1, 10).Select(number => new Computer { Number = number, Name = $"ПК {number}", IsActive = true }).ToList();

        foreach (var computer in computers)
        {
            var available = _sessionManager is null || _sessionManager.IsTimeSlotAvailable(start, duration, computer.Number);
            AddComputerButton(computer.Number, available);
        }

        LayoutComputerMapButtons();
        _computerMap.ResumeLayout();
        UpdateComputerMapSelection();
    }

    private void AddComputerButton(int computerNumber, bool available)
    {
        var index = _computerMap.Controls.Count;
        var button = new Button();
        button.Text = $"ПК {computerNumber}";
        button.Tag = computerNumber;
        button.TabIndex = index;
        _computerAvailability[computerNumber] = available;
        UiTheme.ComputerButton(button, available, computerNumber == SelectedComputerNumber);
        button.Click += (_, _) => _cmbComputers.SelectedValue = computerNumber;
        _computerMap.Controls.Add(button);
    }

    private void LayoutComputerMapButtons()
    {
        var scale = DeviceDpi / 96F;
        var buttonWidth = (int)Math.Round(92 * scale);
        var buttonHeight = (int)Math.Round(46 * scale);
        var spacing = (int)Math.Round(8 * scale);
        var offset = (int)Math.Round(8 * scale);
        var availableWidth = Math.Max(buttonWidth, _computerMap.ClientSize.Width - 2 * offset - SystemInformation.VerticalScrollBarWidth);
        var columns = Math.Max(1, (availableWidth + spacing) / (buttonWidth + spacing));
        var scroll = _computerMap.AutoScrollPosition;
        for (var index = 0; index < _computerMap.Controls.Count; index++)
            _computerMap.Controls[index].SetBounds(scroll.X + offset + index % columns * (buttonWidth + spacing), scroll.Y + offset + index / columns * (buttonHeight + spacing), buttonWidth, buttonHeight);
    }

    private void UpdateComputerMapSelection()
    {
        foreach (Control control in _computerMap.Controls)
        {
            if (control is not Button button || button.Tag is not int number)
                continue;

            UiTheme.ComputerButton(button, _computerAvailability.TryGetValue(number, out var available) && available, number == SelectedComputerNumber);
        }
    }

    private void OnOkClick(object? sender, EventArgs e)
    {
        if (SelectedStart <= DateTime.Now)
        {
            MessageBox.Show("Нельзя забронировать прошедшее время", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (SelectedDuration.TotalMinutes <= 0)
        {
            MessageBox.Show("Длительность должна быть больше 0", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (SelectedComputerNumber <= 0)
        {
            MessageBox.Show("Выберите компьютер", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}
