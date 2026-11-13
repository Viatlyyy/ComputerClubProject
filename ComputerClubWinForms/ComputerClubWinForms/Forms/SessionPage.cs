using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ComputerClubWinForms.Managers;
using ComputerClubWinForms.Models;

namespace ComputerClubWinForms.Forms
{
    public partial class SessionPage : UserControl
    {
        private ClientManager? _clientManager;
        private SessionManager? _sessionManager;
        private bool _isInitialized;
        private string _lastAutoCompletionSummary = string.Empty;
        private bool _clientStatisticsNeedRefresh;
        private readonly Dictionary<int, bool> _computerAvailability = new();

        public event EventHandler? ClientsChanged;
        internal event EventHandler? SessionStatisticsChanged;

        public SessionPage()
        {
            InitializeComponent();
            ArrangePanels();
            EnsureSessionGridColumns();

           
            if (IsDesignMode())
                LoadDesignPreview();
        }

        public SessionPage(ClientManager clientManager, SessionManager sessionManager) : this()
        {
            _clientManager = clientManager;
            _sessionManager = sessionManager;
            ClearDesignPreview();
            Load += SessionPage_Load;
        }

        private void SessionPage_Load(object? sender, EventArgs e)
        {
            InitializeData();
        }

        private void InitializeData()
        {
            if (_isInitialized || _clientManager is null || _sessionManager is null)
                return;

            _isInitialized = true;
            ReloadClients();
            ReloadComputers();
            RefreshAll();
            StartRefreshTimer();
        }

        internal void StartRefreshTimer() => _timer.Start();

        private void LoadDesignPreview()
        {
            var clients = new List<Client>
            {
                new Client { Id = 1, FullName = "Иванов Иван", Phone = "+7 900 111-22-33" },
                new Client { Id = 2, FullName = "Петров Пётр", Phone = "+7 900 222-33-44" }
            };
            _cmbClients.DataSource = clients;
            _cmbClients.DisplayMember = nameof(Client.FullName);
            _cmbClients.ValueMember = nameof(Client.Id);

            var computers = Enumerable.Range(1, 12)
                .Select(number => new Computer { Number = number, Name = $"Компьютер №{number}", IsActive = true })
                .ToList();
            _cmbComputers.DataSource = computers;
            _cmbComputers.DisplayMember = nameof(Computer.Number);
            _cmbComputers.ValueMember = nameof(Computer.Number);

            ShowActiveSessions(new List<Session>
            {
                new Session
                {
                    Id = 1,
                    ClientName = "Иванов Иван",
                    ComputerNumber = 3,
                    StartTime = DateTime.Now.AddMinutes(-35),
                    PlannedEndTime = DateTime.Now.AddMinutes(25)
                },
                new Session
                {
                    Id = 2,
                    ClientName = "Петров Пётр",
                    ComputerNumber = 5,
                    StartTime = DateTime.Now.AddMinutes(-120),
                    PlannedEndTime = DateTime.Now.AddMinutes(-5)
                }
            });

            ShowBookings(new List<Booking>
            {
                new Booking
                {
                    Id = 1,
                    ClientName = "Сидоров Сергей",
                    ComputerNumber = 4,
                    PlannedStart = DateTime.Today.AddDays(1).AddHours(18),
                    Duration = TimeSpan.FromMinutes(120)
                }
            });

            RefreshComputerMap();
        }

        private static bool IsDesignMode()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        }

        private void ClearDesignPreview()
        {
            _cmbClients.DataSource = null;
            _cmbComputers.DataSource = null;
            _gridActiveSessions.DataSource = null;
            _gridBookings.DataSource = null;
            _gridActiveSessions.Rows.Clear();
            _gridBookings.Rows.Clear();
            _computerMap.Controls.Clear();
        }

        private void EnsureSessionGridColumns()
        {
            if (_gridActiveSessions.Columns.Count == 0)
            {
                _gridActiveSessions.AutoGenerateColumns = false;
                _gridActiveSessions.Columns.Add(new DataGridViewTextBoxColumn { Name = nameof(ActiveSessionRow.Id), DataPropertyName = nameof(ActiveSessionRow.Id), HeaderText = "ID", ReadOnly = true });
                _gridActiveSessions.Columns.Add(new DataGridViewTextBoxColumn { Name = nameof(ActiveSessionRow.Client), DataPropertyName = nameof(ActiveSessionRow.Client), HeaderText = "Клиент", ReadOnly = true });
                _gridActiveSessions.Columns.Add(new DataGridViewTextBoxColumn { Name = nameof(ActiveSessionRow.Computer), DataPropertyName = nameof(ActiveSessionRow.Computer), HeaderText = "Компьютер", ReadOnly = true });
                _gridActiveSessions.Columns.Add(new DataGridViewTextBoxColumn { Name = nameof(ActiveSessionRow.StartTime), DataPropertyName = nameof(ActiveSessionRow.StartTime), HeaderText = "Начало", ReadOnly = true });
                _gridActiveSessions.Columns.Add(new DataGridViewTextBoxColumn { Name = nameof(ActiveSessionRow.PlannedEndTime), DataPropertyName = nameof(ActiveSessionRow.PlannedEndTime), HeaderText = "План. окончание", ReadOnly = true });
                _gridActiveSessions.Columns.Add(new DataGridViewTextBoxColumn { Name = nameof(ActiveSessionRow.Remaining), DataPropertyName = nameof(ActiveSessionRow.Remaining), HeaderText = "Осталось", ReadOnly = true });
                _gridActiveSessions.Columns.Add(new DataGridViewCheckBoxColumn { Name = nameof(ActiveSessionRow.IsExpired), DataPropertyName = nameof(ActiveSessionRow.IsExpired), HeaderText = "Время вышло", ReadOnly = true });
            }

            if (_gridBookings.Columns.Count == 0)
            {
                _gridBookings.AutoGenerateColumns = false;
                _gridBookings.Columns.Add(new DataGridViewTextBoxColumn { Name = nameof(BookingRow.Id), DataPropertyName = nameof(BookingRow.Id), HeaderText = "ID", ReadOnly = true });
                _gridBookings.Columns.Add(new DataGridViewTextBoxColumn { Name = nameof(BookingRow.Client), DataPropertyName = nameof(BookingRow.Client), HeaderText = "Клиент", ReadOnly = true });
                _gridBookings.Columns.Add(new DataGridViewTextBoxColumn { Name = nameof(BookingRow.Computer), DataPropertyName = nameof(BookingRow.Computer), HeaderText = "Компьютер", ReadOnly = true });
                _gridBookings.Columns.Add(new DataGridViewTextBoxColumn { Name = nameof(BookingRow.Start), DataPropertyName = nameof(BookingRow.Start), HeaderText = "Начало", ReadOnly = true });
                _gridBookings.Columns.Add(new DataGridViewTextBoxColumn { Name = nameof(BookingRow.End), DataPropertyName = nameof(BookingRow.End), HeaderText = "Окончание", ReadOnly = true });
                _gridBookings.Columns.Add(new DataGridViewTextBoxColumn { Name = nameof(BookingRow.DurationMinutes), DataPropertyName = nameof(BookingRow.DurationMinutes), HeaderText = "Минут", ReadOnly = true });
            }
        }

        public void ReloadClients()
        {
            if (_clientManager is null)
                return;

            var selectedId = SelectedClientId;
            var clients = _clientManager.GetAllClients(_txtClientSearch.Text);
            _cmbClients.DataSource = clients;
            _cmbClients.DisplayMember = nameof(Client.FullName);
            _cmbClients.ValueMember = nameof(Client.Id);

            if (selectedId.HasValue && clients.Any(c => c.Id == selectedId.Value))
                _cmbClients.SelectedValue = selectedId.Value;

            _lblClientHint.Text = clients.Count == 0
                ? "Клиент не найден. Нажмите «Добавить клиента»."
                : string.Empty;
            UpdateActionButtons();
        }

        public void ReloadComputers()
        {
            if (_sessionManager is null)
                return;

            var selectedNumber = SelectedComputerNumber;
            var computers = _sessionManager.GetComputers();
            _cmbComputers.DataSource = computers;
            _cmbComputers.DisplayMember = nameof(Computer.Number);
            _cmbComputers.ValueMember = nameof(Computer.Number);

            if (selectedNumber.HasValue && computers.Any(c => c.Number == selectedNumber.Value))
                _cmbComputers.SelectedValue = selectedNumber.Value;
            UpdateActionButtons();
            RefreshComputerMap();
        }

        private void OnClientSearchChanged(object? sender, EventArgs e)
        {
            try { ReloadClients(); }
            catch (Exception ex)
            {
                _cmbClients.DataSource = null;
                UpdateActionButtons();
                _lblClientHint.Text = "Не удалось найти клиентов. Проверьте соединение с БД.";
                ShowResult(false, "Ошибка поиска: " + ex.Message);
            }
        }

        private void OnClientFormat(object? sender, ListControlConvertEventArgs e)
        {
            if (e.ListItem is Client client)
                e.Value = $"{client.FullName} — {client.Phone}";
        }

        private void OnSelectionChanged(object? sender, EventArgs e)
        {
            UpdateActionButtons();
            UpdateComputerMapSelection();
        }

        private void OnPageSizeChanged(object? sender, EventArgs e) => ArrangePanels();

        private void ArrangePanels()
        {
            if (_mapGroup is null || _editorPanel is null) return;
            var scale = DeviceDpi / 96F;
            var gap = (int)Math.Round(8 * scale);
            var width = Math.Max(0, ClientSize.Width - Padding.Horizontal);
            var editorHeight = (int)Math.Round(104 * scale);
            var mapHeight = (int)Math.Round(144 * scale);
            var minimumTableHeight = (int)Math.Round(122 * scale);
            _editorPanel.SetBounds(Padding.Left, Padding.Top, width, editorHeight);
            var activeTop = _editorPanel.Bottom + gap;
            var mapTop = ClientSize.Height - Padding.Bottom - mapHeight;
            var tablesHeight = Math.Max(2 * minimumTableHeight, mapTop - activeTop - 2 * gap);
            var activeHeight = tablesHeight / 2;
            _activeGroup.SetBounds(Padding.Left, activeTop, width, activeHeight);
            _bookingGroup.SetBounds(Padding.Left, _activeGroup.Bottom + gap, width, tablesHeight - activeHeight);
            _mapGroup.SetBounds(Padding.Left, _bookingGroup.Bottom + gap, width, mapHeight);
            LayoutComputerMapButtons();
        }

        private void UpdateActionButtons()
        {
            var canStart = _clientManager is not null && _sessionManager is not null
                && SelectedClientId.HasValue && SelectedComputerNumber.HasValue;
            _btnStartDirect.Enabled = canStart;
            _btnNewBooking.Enabled = canStart;
        }

        private void OnAddClientClick(object? sender, EventArgs e) => CreateClient();

        private void CreateClient()
        {
            if (_clientManager is null)
                return;
            using var dialog = new ClientEditDialog();
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;
            if (!_clientManager.AddClient(dialog.Client))
            {
                ShowResult(false, _clientManager.LastMessage);
                return;
            }
            _txtClientSearch.Clear();
            ReloadClients();
            _cmbClients.SelectedValue = dialog.Client.Id;
            UpdateActionButtons();
            ClientsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RefreshAll()
        {
            RefreshActiveSessions();
            RefreshBookings();
        }

        private void SessionTimer_Tick(object? sender, EventArgs e)
        {
            RefreshActiveSessions(completeExpired: true);
        }

        private void RefreshActiveSessions(bool completeExpired = false)
        {
            if (_sessionManager is null)
                return;

            try
            {
                var sessions = _sessionManager.GetActiveSessions();
                if (completeExpired)
                {
                    var expiredFound = CompleteExpiredSessions(_sessionManager, sessions);
                    if (_clientStatisticsNeedRefresh)
                    {
                        SessionStatisticsChanged?.Invoke(this, EventArgs.Empty);
                        _clientStatisticsNeedRefresh = false;
                    }
                    if (!Visible)
                        return;
                    if (expiredFound)
                        sessions = _sessionManager.GetActiveSessions();
                }
                ShowActiveSessions(sessions);
                RefreshComputerMap();
            }
            catch (Exception ex)
            {
                if (completeExpired)
                {
                    _activeGroup.Text = "Активные сеансы — не удалось обновить данные, повторяем…";
                    return;
                }
                _timer.Stop();
                MessageBox.Show($"Не удалось обновить список активных сеансов. Проверьте соединение. {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _timer.Start();
            }
        }

        private bool CompleteExpiredSessions(SessionManager manager, List<Session> sessions)
        {
            var cutoff = DateTime.Now;
            var expiredFound = false;
            var completedCount = 0;
            var total = 0m;
            var hasFailure = false;
            foreach (var session in sessions.Where(session => session.PlannedEndTime <= cutoff))
            {
                expiredFound = true;
                var completed = manager.CompleteExpiredSession(session.Id, cutoff);
                if (completed is not null)
                {
                    completedCount++;
                    total += completed.TotalCost;
                }
                else if (manager.LastMessage.Length > 0)
                {
                    hasFailure = true;
                }
            }
            if (completedCount > 0)
            {
                _lastAutoCompletionSummary = $"Автоматически завершено: {completedCount}, сумма: {total:N2} руб.";
                _clientStatisticsNeedRefresh = true;
            }
            var status = hasFailure ? "Не удалось закрыть сеанс, повторяем…" : _lastAutoCompletionSummary;
            var title = status.Length == 0 ? "Активные сеансы" : $"Активные сеансы — {status}";
            if (_activeGroup.Text != title)
                _activeGroup.Text = title;
            return expiredFound;
        }

        private void RefreshBookings()
        {
            if (_sessionManager is null)
                return;

            try
            {
                ShowBookings(_sessionManager.GetActiveBookings());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось обновить список броней: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ShowActiveSessions(List<Session> sessions)
        {
            EnsureSessionGridColumns();
            var hadDataSource = _gridActiveSessions.DataSource is not null;
            var selectedId = GetSelectedActiveSessionRow()?.Id;
            var column = _gridActiveSessions.CurrentCell?.ColumnIndex ?? 0;
            var firstRow = _gridActiveSessions.FirstDisplayedScrollingRowIndex;
            var horizontalOffset = _gridActiveSessions.HorizontalScrollingOffset;
            _gridActiveSessions.DataSource = null;
            _gridActiveSessions.DataSource = sessions.Select(s => new ActiveSessionRow
            {
                Id = s.Id,
                Client = s.ClientName,
                Computer = s.ComputerNumber,
                StartTime = s.StartTime.ToString("dd.MM.yyyy HH:mm"),
                PlannedEndTime = s.PlannedEndTime.ToString("dd.MM.yyyy HH:mm"),
                Remaining = FormatTimeSpan(s.RemainingTime),
                IsExpired = s.RemainingTime.TotalSeconds <= 0
            }).ToList();
            if (hadDataSource)
            {
                _gridActiveSessions.CurrentCell = null;
                _gridActiveSessions.ClearSelection();
                var selectedRow = _gridActiveSessions.Rows.Cast<DataGridViewRow>()
                    .FirstOrDefault(row => row.DataBoundItem is ActiveSessionRow session && session.Id == selectedId);
                if (selectedRow is not null)
                {
                    _gridActiveSessions.CurrentCell = selectedRow.Cells[column];
                    selectedRow.Selected = true;
                }
                if (firstRow >= 0 && _gridActiveSessions.Rows.Count > 0)
                    _gridActiveSessions.FirstDisplayedScrollingRowIndex = Math.Min(firstRow, _gridActiveSessions.Rows.Count - 1);
                _gridActiveSessions.HorizontalScrollingOffset = horizontalOffset;
            }
        }

        public void ShowBookings(List<Booking> bookings)
        {
            EnsureSessionGridColumns();
            var hadDataSource = _gridBookings.DataSource is not null;
            var selectedId = GetSelectedBookingRow()?.Id;
            var column = _gridBookings.CurrentCell?.ColumnIndex ?? 0;
            var firstRow = _gridBookings.FirstDisplayedScrollingRowIndex;
            var horizontalOffset = _gridBookings.HorizontalScrollingOffset;
            _gridBookings.DataSource = null;
            _gridBookings.DataSource = bookings.Select(b => new BookingRow
            {
                Id = b.Id,
                Client = b.ClientName,
                Computer = b.ComputerNumber,
                Start = b.PlannedStart.ToString("dd.MM.yyyy HH:mm"),
                End = b.PlannedEnd.ToString("dd.MM.yyyy HH:mm"),
                DurationMinutes = (int)b.Duration.TotalMinutes
            }).ToList();
            if (hadDataSource)
            {
                _gridBookings.CurrentCell = null;
                _gridBookings.ClearSelection();
                var selectedRow = _gridBookings.Rows.Cast<DataGridViewRow>()
                    .FirstOrDefault(row => row.DataBoundItem is BookingRow booking && booking.Id == selectedId);
                if (selectedRow is not null)
                {
                    _gridBookings.CurrentCell = selectedRow.Cells[column];
                    selectedRow.Selected = true;
                }
                if (firstRow >= 0 && _gridBookings.Rows.Count > 0)
                    _gridBookings.FirstDisplayedScrollingRowIndex = Math.Min(firstRow, _gridBookings.Rows.Count - 1);
                _gridBookings.HorizontalScrollingOffset = horizontalOffset;
            }
        }

        private void RefreshComputerMap()
        {
            var computers = _sessionManager is null
                ? Enumerable.Range(1, 12).Select(number => new Computer { Number = number, Name = $"Компьютер №{number}", IsActive = true }).ToList()
                : _sessionManager.GetComputers();
            var buttons = _computerMap.Controls.OfType<Button>().ToDictionary(button => (int)button.Tag!);
            var listChanged = !buttons.Keys.SequenceEqual(computers.Select(computer => computer.Number));
            var scroll = _computerMap.AutoScrollPosition;
            if (listChanged)
            {
                var numbers = computers.Select(computer => computer.Number).ToHashSet();
                _computerMap.SuspendLayout();
                try
                {
                    foreach (var number in buttons.Keys.Where(number => !numbers.Contains(number)).ToList())
                    {
                        var removed = buttons[number];
                        _computerMap.Controls.Remove(removed);
                        removed.Dispose();
                        buttons.Remove(number);
                        _computerAvailability.Remove(number);
                    }
                    for (var index = 0; index < computers.Count; index++)
                    {
                        var number = computers[index].Number;
                        if (!buttons.TryGetValue(number, out var button))
                        {
                            button = new Button { Text = $"ПК {number}", Tag = number };
                            button.Click += OnComputerButtonClick;
                            _computerMap.Controls.Add(button);
                            buttons.Add(number, button);
                            _computerAvailability.Remove(number);
                        }
                        if (button.TabIndex != index)
                            button.TabIndex = index;
                        if (_computerMap.Controls.GetChildIndex(button) != index)
                            _computerMap.Controls.SetChildIndex(button, index);
                    }
                }
                finally { _computerMap.ResumeLayout(); }
            }

            if (!_cmbComputers.Items.Cast<Computer>().Select(c => c.Number).SequenceEqual(computers.Select(c => c.Number)))
            {
                var selectedNumber = SelectedComputerNumber;
                _cmbComputers.DataSource = computers;
                _cmbComputers.DisplayMember = nameof(Computer.Number);
                _cmbComputers.ValueMember = nameof(Computer.Number);
                if (selectedNumber.HasValue && computers.Any(c => c.Number == selectedNumber.Value))
                    _cmbComputers.SelectedValue = selectedNumber.Value;
                UpdateActionButtons();
            }

            var mapTitle = computers.Count == 0
                ? "В базе данных нет компьютеров. Добавьте записи в таблицу Computers."
                : "Доступность сейчас: зелёный — свободен, красный — занят или забронирован";
            if (_mapGroup.Text != mapTitle)
                _mapGroup.Text = mapTitle;

            foreach (var computer in computers)
            {
                var isAvailableNow = _sessionManager is null || _sessionManager.IsTimeSlotAvailable(DateTime.Now, TimeSpan.FromMinutes(1), computer.Number);
                if (_sessionManager is null && (computer.Number == 3 || computer.Number == 5))
                    isAvailableNow = false;

                var button = buttons[computer.Number];
                var selected = computer.Number == SelectedComputerNumber;
                if (!_computerAvailability.TryGetValue(computer.Number, out var wasAvailable)
                    || wasAvailable != isAvailableNow
                    || button.FlatStyle != (selected ? FlatStyle.Popup : FlatStyle.Standard))
                    UiTheme.ComputerButton(button, isAvailableNow, selected);
                _computerAvailability[computer.Number] = isAvailableNow;
            }

            if (listChanged)
            {
                LayoutComputerMapButtons();
                _computerMap.AutoScrollPosition = new Point(-scroll.X, -scroll.Y);
            }
        }

        private void LayoutComputerMapButtons()
        {
            if (_computerMap is null) return;
            var scale = DeviceDpi / 96F;
            var buttonWidth = (int)Math.Round(92 * scale);
            var buttonHeight = (int)Math.Round(46 * scale);
            var spacing = (int)Math.Round(8 * scale);
            var availableWidth = Math.Max(buttonWidth, _computerMap.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
            var columns = Math.Max(1, (availableWidth + spacing) / (buttonWidth + spacing));
            var scroll = _computerMap.AutoScrollPosition;
            for (var index = 0; index < _computerMap.Controls.Count; index++)
            {
                var button = _computerMap.Controls[index];
                var bounds = new Rectangle(scroll.X + index % columns * (buttonWidth + spacing), scroll.Y + index / columns * (buttonHeight + spacing), buttonWidth, buttonHeight);
                if (button.Bounds != bounds)
                    button.Bounds = bounds;
            }
        }

        private void UpdateComputerMapSelection()
        {
            if (_computerMap is null) return;
            foreach (Control control in _computerMap.Controls)
            {
                if (control is Button button && button.Tag is int number
                    && _computerAvailability.TryGetValue(number, out var available)
                    && button.FlatStyle != (number == SelectedComputerNumber ? FlatStyle.Popup : FlatStyle.Standard))
                    UiTheme.ComputerButton(button, available, number == SelectedComputerNumber);
            }
        }

        private void OnComputerButtonClick(object? sender, EventArgs e)
        {
            if (sender is Button button && button.Tag is int number)
                _cmbComputers.SelectedValue = number;
        }

        private void OnStartDirectClick(object? sender, EventArgs e)
        {
            if (_sessionManager is null)
            {
                ShowResult(false, "Форма открыта в режиме конструктора. Запустите приложение для открытия сеанса.");
                return;
            }

            if (!SelectedClientId.HasValue || !SelectedComputerNumber.HasValue)
            {
                MessageBox.Show("Выберите клиента и компьютер", "Сеанс", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var session = _sessionManager.StartSessionDirect(
                SelectedClientId.Value,
                SelectedComputerNumber.Value,
                DateTime.Now,
                TimeSpan.FromMinutes((double)_numDurationMinutes.Value));

            var success = session is not null;
            ShowResult(success, _sessionManager.LastMessage);
            if (success)
                RefreshAll();
        }

        private void OnCreateBookingClick(object? sender, EventArgs e)
        {
            if (_sessionManager is null)
            {
                ShowResult(false, "Форма открыта в режиме конструктора. Запустите приложение для создания брони.");
                return;
            }

            if (!SelectedClientId.HasValue)
            {
                MessageBox.Show("Выберите клиента перед созданием брони", "Бронь", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dialog = new BookingDialog(_sessionManager, SelectedClientId.Value, SelectedComputerNumber);
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            var success = _sessionManager.CreateBooking(SelectedClientId.Value, dialog.SelectedComputerNumber, dialog.SelectedStart, dialog.SelectedDuration);
            ShowResult(success, _sessionManager.LastMessage);
            if (success)
                RefreshAll();
        }

        private void OnStartBookingClick(object? sender, EventArgs e)
        {
            if (_sessionManager is null)
            {
                ShowResult(false, "Форма открыта в режиме конструктора. Запустите приложение для старта по брони.");
                return;
            }

            var row = GetSelectedBookingRow();
            if (row is null)
            {
                MessageBox.Show("Выберите бронь", "Бронь", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var session = _sessionManager.StartSession(row.Id);
            var success = session is not null;
            ShowResult(success, _sessionManager.LastMessage);
            if (success)
                RefreshAll();
        }

        private void OnCancelBookingClick(object? sender, EventArgs e)
        {
            if (_sessionManager is null)
            {
                ShowResult(false, "Форма открыта в режиме конструктора. Запустите приложение для отмены брони.");
                return;
            }

            var row = GetSelectedBookingRow();
            if (row is null)
            {
                MessageBox.Show("Выберите бронь", "Бронь", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show("Удалить выбранную бронь?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
                return;

            var success = _sessionManager.CancelBooking(row.Id);
            ShowResult(success, _sessionManager.LastMessage);
            if (success)
                RefreshAll();
        }

        private void OnExtendSessionClick(object? sender, EventArgs e)
        {
            if (_sessionManager is null)
            {
                ShowResult(false, "Форма открыта в режиме конструктора. Запустите приложение для продления сеанса.");
                return;
            }

            var row = GetSelectedActiveSessionRow();
            if (row is null)
            {
                MessageBox.Show("Выберите активный сеанс", "Сеанс", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dialog = new ExtendSessionDialog();
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            var success = _sessionManager.ExtendSession(row.Id, TimeSpan.FromMinutes(dialog.AdditionalMinutes));
            ShowResult(success, _sessionManager.LastMessage);
            if (success)
                RefreshAll();
        }

        private void OnFinishSessionClick(object? sender, EventArgs e)
        {
            if (_sessionManager is null)
            {
                ShowResult(false, "Форма открыта в режиме конструктора. Запустите приложение для завершения сеанса.");
                return;
            }

            var row = GetSelectedActiveSessionRow();
            if (row is null)
            {
                MessageBox.Show("Выберите активный сеанс", "Сеанс", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var preview = _sessionManager.PreviewCompletion(row.Id);
            if (preview is null)
            {
                ShowResult(false, _sessionManager.LastMessage);
                return;
            }

            using var receipt = new ReceiptDialog(preview, confirmMode: true);
            if (receipt.ShowDialog(this) != DialogResult.OK)
                return;

            var completedSession = _sessionManager.CompleteSession(row.Id);
            if (completedSession is not null)
            {
                using var finalReceipt = new ReceiptDialog(completedSession, confirmMode: false);
                finalReceipt.ShowDialog(this);
            }
            else
            {
                ShowResult(false, _sessionManager.LastMessage);
            }

            RefreshAll();
        }

        private ActiveSessionRow? GetSelectedActiveSessionRow()
        {
            if (_gridActiveSessions.CurrentRow?.DataBoundItem is ActiveSessionRow row)
                return row;

            if (_gridActiveSessions.CurrentRow is not null && _gridActiveSessions.CurrentRow.Cells[0].Value is not null)
            {
                return new ActiveSessionRow
                {
                    Id = Convert.ToInt32(_gridActiveSessions.CurrentRow.Cells[0].Value)
                };
            }

            return null;
        }

        private BookingRow? GetSelectedBookingRow()
        {
            if (_gridBookings.CurrentRow?.DataBoundItem is BookingRow row)
                return row;

            if (_gridBookings.CurrentRow is not null && _gridBookings.CurrentRow.Cells[0].Value is not null)
            {
                return new BookingRow
                {
                    Id = Convert.ToInt32(_gridBookings.CurrentRow.Cells[0].Value)
                };
            }

            return null;
        }

        private int? SelectedClientId
        {
            get
            {
                if (_cmbClients.SelectedValue is int id)
                    return id;
                if (int.TryParse(_cmbClients.SelectedValue?.ToString(), out var parsed))
                    return parsed;
                return null;
            }
        }

        private int? SelectedComputerNumber
        {
            get
            {
                if (_cmbComputers.SelectedValue is int number)
                    return number;
                if (int.TryParse(_cmbComputers.SelectedValue?.ToString(), out var parsed))
                    return parsed;
                return null;
            }
        }

        private static string FormatTimeSpan(TimeSpan value)
        {
            var negative = value.TotalSeconds < 0;
            if (negative)
                value = value.Negate();
            return $"{(negative ? "-" : string.Empty)}{(int)value.TotalHours:00}:{value.Minutes:00}:{value.Seconds:00}";
        }

        private static void ShowResult(bool success, string message)
        {
            MessageBox.Show(message, success ? "Готово" : "Ошибка", MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        private void ActiveSessionsCellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            var row = _gridActiveSessions.Rows[e.RowIndex].DataBoundItem as ActiveSessionRow;
            var isExpired = row?.IsExpired == true;

            if (row is null && _gridActiveSessions.Rows[e.RowIndex].Cells[6].Value is bool cellValue)
                isExpired = cellValue;

            if (isExpired)
                _gridActiveSessions.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.MistyRose;
            else
                _gridActiveSessions.Rows[e.RowIndex].DefaultCellStyle.BackColor = _gridActiveSessions.DefaultCellStyle.BackColor;
        }

        private class ActiveSessionRow
        {
            public int Id { get; set; }
            public string Client { get; set; } = string.Empty;
            public int Computer { get; set; }
            public string StartTime { get; set; } = string.Empty;
            public string PlannedEndTime { get; set; } = string.Empty;
            public string Remaining { get; set; } = string.Empty;
            public bool IsExpired { get; set; }
        }

        private class BookingRow
        {
            public int Id { get; set; }
            public string Client { get; set; } = string.Empty;
            public int Computer { get; set; }
            public string Start { get; set; } = string.Empty;
            public string End { get; set; } = string.Empty;
            public int DurationMinutes { get; set; }
        }
    }
}
