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

        public SessionPage()
        {
            InitializeComponent();
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
            _timer.Start();
        }

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
            var clients = _clientManager.GetAllClients();
            _cmbClients.DataSource = clients;
            _cmbClients.DisplayMember = nameof(Client.FullName);
            _cmbClients.ValueMember = nameof(Client.Id);

            if (selectedId.HasValue)
                _cmbClients.SelectedValue = selectedId.Value;

            _btnStartDirect.Enabled = clients.Count > 0;
            _btnNewBooking.Enabled = clients.Count > 0;
        }

        private void ReloadComputers()
        {
            if (_sessionManager is null)
                return;

            var computers = _sessionManager.GetComputers();
            _cmbComputers.DataSource = computers;
            _cmbComputers.DisplayMember = nameof(Computer.Number);
            _cmbComputers.ValueMember = nameof(Computer.Number);
        }

        private void RefreshAll()
        {
            RefreshActiveSessions();
            RefreshBookings();
            RefreshComputerMap();
        }

        private void SessionTimer_Tick(object? sender, EventArgs e)
        {
            RefreshActiveSessions();
        }

        private void RefreshActiveSessions()
        {
            if (_sessionManager is null)
                return;

            try
            {
                var sessions = _sessionManager.GetActiveSessions();
                ShowActiveSessions(sessions);
                RefreshComputerMap();
            }
            catch (Exception ex)
            {
                _timer.Stop();
                MessageBox.Show($"Не удалось обновить список активных сеансов. Проверьте соединение. {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _timer.Start();
            }
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
        }

        public void ShowBookings(List<Booking> bookings)
        {
            EnsureSessionGridColumns();
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
        }

        private void RefreshComputerMap()
        {
            _computerMap.SuspendLayout();
            _computerMap.Controls.Clear();

            var computers = _sessionManager is null
                ? Enumerable.Range(1, 12).Select(number => new Computer { Number = number, Name = $"Компьютер №{number}", IsActive = true }).ToList()
                : _sessionManager.GetComputers();

            foreach (var computer in computers)
            {
                var isAvailableNow = _sessionManager is null || _sessionManager.IsTimeSlotAvailable(DateTime.Now, TimeSpan.FromMinutes(1), computer.Number);
                if (_sessionManager is null && (computer.Number == 3 || computer.Number == 5))
                    isAvailableNow = false;

                const int buttonWidth = 82;
                const int buttonHeight = 46;
                const int spacing = 10;
                const int startOffset = 8;

                var index = _computerMap.Controls.Count;
                var columns = Math.Max(1, (_computerMap.ClientSize.Width - startOffset) / (buttonWidth + spacing));
                var row = index / columns;
                var column = index % columns;

                var button = new Button();
                button.Text = $"ПК {computer.Number}";
                button.Size = new Size(buttonWidth, buttonHeight);
                button.Location = new Point(startOffset + column * (buttonWidth + spacing), startOffset + row * (buttonHeight + spacing));
                button.BackColor = isAvailableNow ? Color.LightGreen : Color.LightCoral;
                button.UseVisualStyleBackColor = false;
                button.Tag = computer.Number;
                button.Click += OnComputerButtonClick;
                _computerMap.Controls.Add(button);
            }

            _computerMap.ResumeLayout();
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
