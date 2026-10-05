using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using ComputerClubWinForms.Managers;
using ComputerClubWinForms.Models;

namespace ComputerClubWinForms.Forms
{
    public partial class ClientPage : UserControl
    {
        private ClientManager? _clientManager;

        public event EventHandler? ClientsChanged;

        public ClientPage()
        {
            InitializeComponent();
            EnsureClientGridColumns();

            
            if (IsDesignMode())
                LoadDesignPreview();
        }

        public ClientPage(ClientManager clientManager) : this()
        {
            _clientManager = clientManager;
            _gridClients.DataSource = null;
            _gridClients.Rows.Clear();
            Load += ClientPage_Load;
        }

        private void ClientPage_Load(object? sender, EventArgs e)
        {
            LoadClients();
        }

        private void LoadDesignPreview()
        {
            ShowRows(new List<ClientGridRow>
            {
                new ClientGridRow { Id = 1, FullName = "Иванов Иван", Phone = "+7 900 111-22-33", TotalHours = 12.5, TotalSpent = 1875m, DiscountPercent = 5 },
                new ClientGridRow { Id = 2, FullName = "Петров Пётр", Phone = "+7 900 222-33-44", TotalHours = 31.0, TotalSpent = 4650m, DiscountPercent = 15 }
            });
        }

        private static bool IsDesignMode()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        }

        private void EnsureClientGridColumns()
        {
            if (_gridClients.Columns.Count > 0)
                return;

            _gridClients.AutoGenerateColumns = false;
            _gridClients.Columns.Add(new DataGridViewTextBoxColumn { Name = nameof(ClientGridRow.Id), DataPropertyName = nameof(ClientGridRow.Id), HeaderText = "ID", MinimumWidth = 50, ReadOnly = true });
            _gridClients.Columns.Add(new DataGridViewTextBoxColumn { Name = nameof(ClientGridRow.FullName), DataPropertyName = nameof(ClientGridRow.FullName), HeaderText = "ФИО", ReadOnly = true });
            _gridClients.Columns.Add(new DataGridViewTextBoxColumn { Name = nameof(ClientGridRow.Phone), DataPropertyName = nameof(ClientGridRow.Phone), HeaderText = "Телефон", ReadOnly = true });
            _gridClients.Columns.Add(new DataGridViewTextBoxColumn { Name = nameof(ClientGridRow.TotalHours), DataPropertyName = nameof(ClientGridRow.TotalHours), HeaderText = "Всего часов", ReadOnly = true });
            _gridClients.Columns.Add(new DataGridViewTextBoxColumn { Name = nameof(ClientGridRow.TotalSpent), DataPropertyName = nameof(ClientGridRow.TotalSpent), HeaderText = "Всего потрачено", ReadOnly = true });
            _gridClients.Columns.Add(new DataGridViewTextBoxColumn { Name = nameof(ClientGridRow.DiscountPercent), DataPropertyName = nameof(ClientGridRow.DiscountPercent), HeaderText = "Скидка, %", ReadOnly = true });
        }

        private void OnSearchTextChanged(object? sender, EventArgs e)
        {
            if (_clientManager is not null)
                LoadClients();
        }

        private void LoadClients()
        {
            if (_clientManager is null)
                return;

            try
            {
                var clients = _clientManager.GetAllClients(_txtSearch.Text);
                ShowClients(clients);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить клиентов: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ShowClients(List<Client> clients)
        {
            var rows = clients.Select(c => new ClientGridRow
            {
                Id = c.Id,
                FullName = c.FullName,
                Phone = c.Phone,
                TotalHours = Math.Round(c.TotalHours, 2),
                TotalSpent = c.TotalSpent,
                DiscountPercent = c.DiscountPercent
            }).ToList();

            ShowRows(rows);
        }

        private void ShowRows(List<ClientGridRow> rows)
        {
            EnsureClientGridColumns();
            _gridClients.DataSource = null;
            _gridClients.DataSource = rows;
        }

        private void OnAddClientClick(object? sender, EventArgs e)
        {
            if (_clientManager is null)
            {
                ShowMessage(false, "Форма открыта в режиме конструктора. Запустите приложение для работы с клиентами.");
                return;
            }

            using var dialog = new ClientEditDialog();
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            var success = _clientManager.AddClient(dialog.Client);
            ShowMessage(success, _clientManager.LastMessage);
            if (success)
            {
                LoadClients();
                ClientsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnEditClientClick(object? sender, EventArgs e)
        {
            if (_clientManager is null)
            {
                ShowMessage(false, "Форма открыта в режиме конструктора. Запустите приложение для редактирования клиентов.");
                return;
            }

            var row = GetSelectedRow();
            if (row is null)
            {
                MessageBox.Show("Выберите клиента для редактирования", "Клиенты", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var client = _clientManager.GetClientById(row.Id);
            if (client is null)
            {
                MessageBox.Show("Клиент не найден", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadClients();
                return;
            }

            using var dialog = new ClientEditDialog(client);
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            var success = _clientManager.EditClient(dialog.Client);
            ShowMessage(success, _clientManager.LastMessage);
            if (success)
            {
                LoadClients();
                ClientsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnDeleteClientClick(object? sender, EventArgs e)
        {
            if (_clientManager is null)
            {
                ShowMessage(false, "Форма открыта в режиме конструктора. Запустите приложение для удаления клиентов.");
                return;
            }

            var row = GetSelectedRow();
            if (row is null)
            {
                MessageBox.Show("Выберите клиента для удаления", "Клиенты", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show($"Удалить клиента \"{row.FullName}\"?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
                return;

            var success = _clientManager.DeleteClient(row.Id);
            ShowMessage(success, _clientManager.LastMessage);
            if (success)
            {
                LoadClients();
                ClientsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void ShowMessage(bool success, string message)
        {
            MessageBox.Show(message, success ? "Готово" : "Ошибка", MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        private ClientGridRow? GetSelectedRow()
        {
            if (_gridClients.CurrentRow?.DataBoundItem is ClientGridRow row)
                return row;

            if (_gridClients.CurrentRow is not null && _gridClients.CurrentRow.Cells[0].Value is not null)
            {
                return new ClientGridRow
                {
                    Id = Convert.ToInt32(_gridClients.CurrentRow.Cells[0].Value),
                    FullName = Convert.ToString(_gridClients.CurrentRow.Cells[1].Value) ?? string.Empty
                };
            }

            return null;
        }

        private void OnGridClientsCellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                OnEditClientClick(sender, EventArgs.Empty);
        }

        private class ClientGridRow
        {
            public int Id { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public double TotalHours { get; set; }
            public decimal TotalSpent { get; set; }
            public int DiscountPercent { get; set; }
        }
    }
}
