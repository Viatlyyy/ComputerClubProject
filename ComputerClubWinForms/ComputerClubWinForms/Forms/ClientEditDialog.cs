using System;
using System.Linq;
using System.Runtime.InteropServices;
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
        UiTheme.Apply(this);
        _txtFullName.Font = new System.Drawing.Font("Segoe UI", 9F);
        _txtPhone.Font = new System.Drawing.Font("Segoe UI", 9F);
        UiTheme.Action(_btnOk, primary: true);
        Text = Client.Id == 0 ? "Добавить клиента" : "Редактировать клиента";
        _txtFullName.Text = Client.FullName;
        if (ComputerClubWinForms.Models.Client.TryGetRussianPhoneDigits(Client.Phone, out var digits))
            _txtPhone.Text = ClientPhoneTextBox.FormatNumber(digits);
        else if (!string.IsNullOrWhiteSpace(Client.Phone))
            _phoneHint.Text = "Введённый ранее номер требуется привести к формату +7.";
    }

    private void OnPhoneTextChanged(object? sender, EventArgs e)
    {
        if (_txtPhone.MaskCompleted)
            _phoneHint.Text = string.Empty;
    }

    private void OnOkClick(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtFullName.Text))
        {
            MessageBox.Show("Введите ФИО клиента", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtFullName.Focus();
            return;
        }

        if (!_txtPhone.MaskCompleted)
        {
            _phoneHint.Text = "Введите все 10 цифр номера после +7.";
            _txtPhone.Focus();
            return;
        }

        Client.FullName = _txtFullName.Text.Trim();
        Client.Phone = "7" + _txtPhone.Text;
        DialogResult = DialogResult.OK;
        Close();
    }
}

public sealed class ClientPhoneTextBox : MaskedTextBox
{
    internal static string FormatNumber(string digits) =>
        $"+7 ({digits[..3]}) {digits[3..6]}-{digits[6..8]}-{digits[8..10]}";

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if ((keyData == (Keys.Control | Keys.V) || keyData == (Keys.Shift | Keys.Insert)) && TryPasteNumber())
            return true;
        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void WndProc(ref Message m)
    {
        const int pasteMessage = 0x0302; // WM_PASTE
        if (m.Msg != pasteMessage || !TryPasteNumber())
            base.WndProc(ref m);
    }

    private bool TryPasteNumber()
    {
        if (ReadOnly)
            return false;
        string text;
        try
        {
            text = Clipboard.GetText();
        }
        catch (ExternalException)
        {
            return false;
        }

        if (!Client.TryGetRussianPhoneDigits(text, out var digits))
            return text.Count(char.IsAsciiDigit) >= 10;

        Text = FormatNumber(digits);
        SelectionStart = Mask.Length;
        SelectionLength = 0;
        return true;
    }
}
