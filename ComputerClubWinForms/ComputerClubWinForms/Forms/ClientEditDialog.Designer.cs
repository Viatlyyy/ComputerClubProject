#nullable enable

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ComputerClubWinForms.Forms;

partial class ClientEditDialog
{
    private Label _fullNameLabel = null!;
    private Label _phoneLabel = null!;
    private TextBox _txtFullName = null!;
    private TextBox _txtPhone = null!;
    private Button _btnOk = null!;
    private Button _btnCancel = null!;

    private void InitializeComponent()
    {
        _fullNameLabel = new Label();
        _phoneLabel = new Label();
        _txtFullName = new TextBox();
        _txtPhone = new TextBox();
        _btnOk = new Button();
        _btnCancel = new Button();
        SuspendLayout();

        _fullNameLabel.AutoSize = true;
        _fullNameLabel.Location = new Point(26, 30);
        _fullNameLabel.Name = "_fullNameLabel";
        _fullNameLabel.Size = new Size(34, 15);
        _fullNameLabel.TabIndex = 0;
        _fullNameLabel.Text = "ФИО";

        _txtFullName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _txtFullName.Location = new Point(145, 26);
        _txtFullName.Name = "_txtFullName";
        _txtFullName.Size = new Size(284, 23);
        _txtFullName.TabIndex = 1;

        _phoneLabel.AutoSize = true;
        _phoneLabel.Location = new Point(26, 70);
        _phoneLabel.Name = "_phoneLabel";
        _phoneLabel.Size = new Size(52, 15);
        _phoneLabel.TabIndex = 2;
        _phoneLabel.Text = "Телефон";

        _txtPhone.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _txtPhone.Location = new Point(145, 66);
        _txtPhone.Name = "_txtPhone";
        _txtPhone.Size = new Size(284, 23);
        _txtPhone.TabIndex = 3;

        _btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _btnOk.Location = new Point(204, 160);
        _btnOk.Name = "_btnOk";
        _btnOk.Size = new Size(108, 27);
        _btnOk.TabIndex = 4;
        _btnOk.Text = "Сохранить";
        _btnOk.UseVisualStyleBackColor = true;
        _btnOk.Click += OnOkClick;

        _btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _btnCancel.DialogResult = DialogResult.Cancel;
        _btnCancel.Location = new Point(318, 160);
        _btnCancel.Name = "_btnCancel";
        _btnCancel.Size = new Size(108, 27);
        _btnCancel.TabIndex = 5;
        _btnCancel.Text = "Отмена";
        _btnCancel.UseVisualStyleBackColor = true;

        AcceptButton = _btnOk;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = _btnCancel;
        ClientSize = new Size(456, 210);
        Controls.AddRange(new Control[]
        {
            _fullNameLabel,
            _txtFullName,
            _phoneLabel,
            _txtPhone,
            _btnOk,
            _btnCancel
        });
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(472, 249);
        Name = "ClientEditDialog";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Добавить клиента";
        ResumeLayout(false);
        PerformLayout();
    }
}
