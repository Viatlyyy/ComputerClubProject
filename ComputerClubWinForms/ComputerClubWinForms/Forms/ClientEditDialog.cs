using System;
using System.Windows.Forms;
using ComputerClubWinForms.Models;

namespace ComputerClubWinForms.Forms;

public partial class ClientEditDialog : Form
{
    public Client Client { get; private set; }

    public ClientEditDialog() : this(null)
    {
    }

    public ClientEditDialog(Client? client)
    {
        Client = client is null
            ? new Client()
            : new Client
            {
                Id = client.Id,
                FullName = client.FullName,
                Phone = client.Phone,
                TotalHours = client.TotalHours,
                TotalSpent = client.TotalSpent,
                DiscountPercent = client.DiscountPercent
            };

        InitializeComponent();
        Text = Client.Id == 0 ? "Добавить клиента" : "Редактировать клиента";
        _txtFullName.Text = Client.FullName;
        _txtPhone.Text = Client.Phone;
    }

    private void OnOkClick(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtFullName.Text))
        {
            MessageBox.Show("Введите ФИО клиента", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtFullName.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(_txtPhone.Text))
        {
            MessageBox.Show("Введите телефон клиента", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtPhone.Focus();
            return;
        }

        Client.FullName = _txtFullName.Text.Trim();
        Client.Phone = _txtPhone.Text.Trim();
        DialogResult = DialogResult.OK;
        Close();
    }
}
