#nullable enable

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ComputerClubWinForms.Forms;

partial class ExtendSessionDialog
{
    private Label _lblMinutes = null!;
    private NumericUpDown _numMinutes = null!;
    private Button _btnOk = null!;
    private Button _btnCancel = null!;

    private void InitializeComponent()
    {
        _lblMinutes = new Label();
        _numMinutes = new NumericUpDown();
        _btnOk = new Button();
        _btnCancel = new Button();
        ((System.ComponentModel.ISupportInitialize)_numMinutes).BeginInit();
        SuspendLayout();

        _lblMinutes.AutoSize = true;
        _lblMinutes.Location = new Point(24, 26);
        _lblMinutes.Name = "_lblMinutes";
        _lblMinutes.Size = new Size(141, 15);
        _lblMinutes.TabIndex = 0;
        _lblMinutes.Text = "Добавить минут к сеансу";

        _numMinutes.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _numMinutes.Location = new Point(24, 58);
        _numMinutes.Maximum = new decimal(new int[] { 1440, 0, 0, 0 });
        _numMinutes.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        _numMinutes.Name = "_numMinutes";
        _numMinutes.Size = new Size(372, 28);
        _numMinutes.TabIndex = 1;
        _numMinutes.Value = new decimal(new int[] { 30, 0, 0, 0 });

        _btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _btnOk.DialogResult = DialogResult.OK;
        _btnOk.Location = new Point(124, 132);
        _btnOk.Name = "_btnOk";
        _btnOk.Size = new Size(130, 42);
        _btnOk.TabIndex = 2;
        _btnOk.Text = "Продлить";
        _btnOk.UseVisualStyleBackColor = true;

        _btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _btnCancel.DialogResult = DialogResult.Cancel;
        _btnCancel.Location = new Point(266, 132);
        _btnCancel.Name = "_btnCancel";
        _btnCancel.Size = new Size(130, 42);
        _btnCancel.TabIndex = 3;
        _btnCancel.Text = "Отмена";
        _btnCancel.UseVisualStyleBackColor = true;

        AcceptButton = _btnOk;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Font = new Font("Segoe UI", 9F);
        CancelButton = _btnCancel;
        ClientSize = new Size(420, 198);
        Controls.AddRange(new Control[]
        {
            _lblMinutes,
            _numMinutes,
            _btnOk,
            _btnCancel
        });
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(436, 237);
        Name = "ExtendSessionDialog";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Продление сеанса";
        ((System.ComponentModel.ISupportInitialize)_numMinutes).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
