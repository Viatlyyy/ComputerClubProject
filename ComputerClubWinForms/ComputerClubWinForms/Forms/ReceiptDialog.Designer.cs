#nullable enable

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ComputerClubWinForms.Forms;

partial class ReceiptDialog
{
    private System.ComponentModel.IContainer? components = null;
    private TextBox _textBox = null!;
    private Button _btnOk = null!;
    private Button _btnCancel = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _textBox = new TextBox();
        _btnOk = new Button();
        _btnCancel = new Button();
        SuspendLayout();

        _textBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _textBox.Font = new Font("Consolas", 10F, FontStyle.Regular, GraphicsUnit.Point);
        _textBox.Location = new Point(16, 16);
        _textBox.Multiline = true;
        _textBox.Name = "_textBox";
        _textBox.ReadOnly = true;
        _textBox.ScrollBars = ScrollBars.Vertical;
        _textBox.Size = new Size(528, 382);
        _textBox.TabIndex = 0;

        _btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _btnOk.DialogResult = DialogResult.OK;
        _btnOk.Location = new Point(314, 418);
        _btnOk.Name = "_btnOk";
        _btnOk.Size = new Size(150, 27);
        _btnOk.TabIndex = 1;
        _btnOk.Text = "ОК";
        _btnOk.UseVisualStyleBackColor = true;

        _btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _btnCancel.DialogResult = DialogResult.Cancel;
        _btnCancel.Location = new Point(470, 418);
        _btnCancel.Name = "_btnCancel";
        _btnCancel.Size = new Size(74, 27);
        _btnCancel.TabIndex = 2;
        _btnCancel.Text = "Отмена";
        _btnCancel.UseVisualStyleBackColor = true;
        _btnCancel.Visible = false;

        AcceptButton = _btnOk;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = _btnOk;
        ClientSize = new Size(560, 460);
        Controls.AddRange(new Control[] { _textBox, _btnOk, _btnCancel });
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(576, 499);
        Name = "ReceiptDialog";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Чек закрытого сеанса";
        ResumeLayout(false);
        PerformLayout();
    }
}
