using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ComputerClubWinForms.Models;

namespace ComputerClubWinForms.Forms;

public partial class SessionPage
{
    private TabControl _sessionTabs = null!;
    private TabPage _pendingPage = null!;
    private DataGridView _gridPendingPayments = null!;
    private DataGridView _gridSessionHistory = null!;
    private DateTimePicker _historyFrom = null!;
    private DateTimePicker _historyTo = null!;
    private TextBox _historySearch = null!;
    private Label _paymentStatus = null!;
    private Label _historyStatus = null!;
    private List<PaymentRow> _pendingRows = new();
    private List<PaymentRow> _historyRows = new();

    private void InitializePaymentControls()
    {
        _sessionTabs = new TabControl { Name = "sessionTabs" };
        UiTheme.Tabs(_sessionTabs);
        var activePage = new TabPage("Активные сеансы") { UseVisualStyleBackColor = true };
        var bookingPage = new TabPage("Бронирования") { UseVisualStyleBackColor = true };
        _pendingPage = new TabPage("Ожидают оплаты") { UseVisualStyleBackColor = true };
        var historyPage = new TabPage("История сеансов") { UseVisualStyleBackColor = true };
        foreach (var page in new[] { activePage, bookingPage, _pendingPage, historyPage })
            page.Font = UiTheme.BodyFont;
        _sessionTabs.TabPages.AddRange(new[] { activePage, bookingPage, _pendingPage, historyPage });
        Controls.Add(_sessionTabs);
        activePage.Controls.Add(_activeGroup);
        bookingPage.Controls.Add(_bookingGroup);
        _activeGroup.Dock = DockStyle.Fill;
        _bookingGroup.Dock = DockStyle.Fill;

        _gridPendingPayments = CreatePaymentGrid();
        _gridSessionHistory = CreatePaymentGrid();
        _paymentStatus = new Label { AutoSize = true, Margin = new Padding(8, 12, 0, 0) };
        var pendingActions = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(4),
            WrapContents = false, AutoScroll = true
        };
        var pay = new Button { Text = "Принять оплату", Width = 160, Height = 36 };
        var pendingRefresh = new Button { Text = "Обновить", Width = 120, Height = 36 };
        pay.Click += (_, _) => OpenSelectedReceipt(_gridPendingPayments, true);
        pendingRefresh.Click += (_, _) => RefreshPaymentLists();
        pendingActions.Controls.AddRange(new Control[] { pay, pendingRefresh, _paymentStatus });
        _pendingPage.Controls.Add(_gridPendingPayments);
        _pendingPage.Controls.Add(pendingActions);

        var filters = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, Height = 48, Padding = new Padding(4), WrapContents = false,
            AutoScroll = true
        };
        _historyFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 112, Value = DateTime.Today.AddMonths(-1) };
        _historyTo = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 112, Value = DateTime.Today };
        _historySearch = new TextBox { Width = 220, PlaceholderText = "ФИО, телефон или № сеанса" };
        var search = new Button { Text = "Найти", Width = 100, Height = 30 };
        filters.Controls.AddRange(new Control[]
        {
            new Label { Text = "Завершён с", AutoSize = true, Margin = new Padding(3, 7, 3, 0) }, _historyFrom,
            new Label { Text = "по", AutoSize = true, Margin = new Padding(3, 7, 3, 0) }, _historyTo,
            _historySearch, search
        });
        search.Click += (_, _) => RefreshPaymentLists();
        _historySearch.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            RefreshPaymentLists();
        };
        var historyActions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(4), WrapContents = false, AutoScroll = true };
        _historyStatus = new Label { AutoSize = true, Margin = new Padding(8, 12, 0, 0) };
        var receipt = new Button { Text = "Открыть чек завершённого сеанса", Width = 250, Height = 36 };
        receipt.Click += (_, _) => OpenSelectedReceipt(_gridSessionHistory, false);
        historyActions.Controls.AddRange(new Control[] { receipt, _historyStatus });
        historyPage.Controls.Add(_gridSessionHistory);
        historyPage.Controls.Add(filters);
        historyPage.Controls.Add(historyActions);
        _sessionTabs.SelectedIndexChanged += (_, _) => RefreshPaymentLists();
    }

    private static DataGridView CreatePaymentGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoGenerateColumns = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        UiTheme.Grid(grid);
        foreach (var (name, title, width, format) in new[]
        {
            (nameof(PaymentRow.Id), "ID", 44, ""),
            (nameof(PaymentRow.Client), "Клиент", 230, ""),
            (nameof(PaymentRow.Computer), "ПК", 54, ""),
            (nameof(PaymentRow.EndTime), "Завершён", 145, "dd.MM.yyyy HH:mm"),
            (nameof(PaymentRow.Amount), "Сумма, руб.", 95, "N2"),
            (nameof(PaymentRow.Status), "Оплата", 130, ""),
            (nameof(PaymentRow.PaidAt), "Оплачен", 145, "dd.MM.yyyy HH:mm")
        })
        {
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = name, DataPropertyName = name, HeaderText = title,
                AutoSizeMode = name == nameof(PaymentRow.Client)
                    ? DataGridViewAutoSizeColumnMode.Fill : DataGridViewAutoSizeColumnMode.None,
                MinimumWidth = width, Width = width,
                DefaultCellStyle = new DataGridViewCellStyle { Format = format, NullValue = "—" },
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
        }
        return grid;
    }

    private void RefreshPaymentLists()
    {
        if (_sessionManager is null || _sessionTabs is null) return;
        try
        {
            var pending = _sessionManager.GetSessionHistory(DateTime.Today, DateTime.Today, true);
            BindPaymentRows(_gridPendingPayments, ref _pendingRows, pending);
            _pendingPage.Text = $"Ожидают оплаты ({pending.Count})";
            _paymentStatus.Text = $"К оплате: {pending.Sum(s => s.TotalCost):N2} руб.";
            if (_sessionTabs.SelectedIndex != 3) return;
            if (_historyFrom.Value.Date > _historyTo.Value.Date)
            {
                _historyStatus.Text = "Начальная дата позже конечной";
                BindPaymentRows(_gridSessionHistory, ref _historyRows, new List<Session>());
                return;
            }
            var history = _sessionManager.GetSessionHistory(_historyFrom.Value, _historyTo.Value, false, _historySearch.Text);
            BindPaymentRows(_gridSessionHistory, ref _historyRows, history);
            _historyStatus.Text = $"Завершённых сеансов: {history.Count}";
        }
        catch (Exception)
        {
            _pendingPage.Text = "Ожидают оплаты — ошибка обновления";
            _paymentStatus.Text = "Не удалось обновить список. Проверьте подключение.";
            _historyStatus.Text = "Ошибка обновления. Проверьте подключение.";
        }
    }

    private static void BindPaymentRows(DataGridView grid, ref List<PaymentRow> previous, List<Session> sessions)
    {
        var rows = sessions.Select(s => new PaymentRow(s.Id, s.ClientName, s.ComputerNumber,
            s.EndTime, s.TotalCost, s.PaidAt.HasValue ? "Оплачено" : "Ожидает оплаты", s.PaidAt)).ToList();
        if (grid.DataSource is not null && rows.SequenceEqual(previous)) return;
        var selectedId = (grid.CurrentRow?.DataBoundItem as PaymentRow)?.Id;
        var scroll = grid.FirstDisplayedScrollingRowIndex;
        var horizontal = grid.HorizontalScrollingOffset;
        previous = rows;
        grid.DataSource = rows;
        grid.CurrentCell = null;
        grid.ClearSelection();
        var selected = grid.Rows.Cast<DataGridViewRow>()
            .FirstOrDefault(row => (row.DataBoundItem as PaymentRow)?.Id == selectedId);
        if (selected is not null)
        {
            grid.CurrentCell = selected.Cells[1];
            selected.Selected = true;
        }
        if (scroll >= 0 && grid.Rows.Count > 0)
            grid.FirstDisplayedScrollingRowIndex = Math.Min(scroll, grid.Rows.Count - 1);
        grid.HorizontalScrollingOffset = horizontal;
    }

    private void OpenSelectedReceipt(DataGridView grid, bool confirmPayment)
    {
        if (grid.CurrentRow?.DataBoundItem is not PaymentRow row)
        {
            MessageBox.Show("Выберите сеанс в таблице.", "Оплата", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        OpenSessionReceipt(row.Id, confirmPayment);
    }

    private void OpenSessionReceipt(int sessionId, bool confirmPayment)
    {
        if (_sessionManager is null) return;
        try
        {
            var session = _sessionManager.GetSessionById(sessionId);
            if (session is null || !session.IsCompleted)
            {
                ShowResult(false, "Завершённый сеанс не найден");
                return;
            }
            using var dialog = new ReceiptDialog(session, confirmPayment && !session.PaidAt.HasValue);
            if (dialog.ShowDialog(this) != DialogResult.OK || !confirmPayment || session.PaidAt.HasValue)
                return;
            var paid = _sessionManager.ConfirmPayment(sessionId);
            if (paid is null)
            {
                ShowResult(false, _sessionManager.LastMessage);
                return;
            }
            _clientStatisticsNeedRefresh = true;
            using var receipt = new ReceiptDialog(paid, confirmMode: false);
            receipt.ShowDialog(this);
        }
        catch (Exception ex)
        {
            ShowResult(false, "Не удалось открыть расчёт: " + ex.Message);
        }
        finally
        {
            RefreshPaymentLists();
        }
    }

    private sealed record PaymentRow(int Id, string Client, int Computer, DateTime? EndTime,
        decimal Amount, string Status, DateTime? PaidAt);
}
