#nullable enable

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ComputerClubWinForms.Forms;

partial class ComparePeriodDialog
{
    private Label _startLabel = null!;
    private Label _endLabel = null!;
    private DateTimePicker _dtpOtherStart = null!;
    private DateTimePicker _dtpOtherEnd = null!;
    private Button _btnOk = null!;
    private Button _btnCancel = null!;

    private void InitializeComponent()
    {
        _startLabel = new Label();
        _endLabel = new Label();
        _dtpOtherStart = new DateTimePicker();
        _dtpOtherEnd = new DateTimePicker();
        _btnOk = new Button();
        _btnCancel = new Button();
        SuspendLayout();

        _startLabel.AutoSize = true;
        _startLabel.Location = new Point(26, 28);
        _startLabel.Name = "_startLabel";
        _startLabel.Size = new Size(47, 15);
        _startLabel.TabIndex = 0;
        _startLabel.Text = "Начало";

        _dtpOtherStart.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _dtpOtherStart.Format = DateTimePickerFormat.Short;
        _dtpOtherStart.Location = new Point(155, 24);
        _dtpOtherStart.Name = "_dtpOtherStart";
        _dtpOtherStart.Size = new Size(264, 23);
        _dtpOtherStart.TabIndex = 1;
        _dtpOtherStart.Value = new DateTime(2026, 5, 1, 0, 0, 0, 0);

        _endLabel.AutoSize = true;
        _endLabel.Location = new Point(26, 64);
        _endLabel.Name = "_endLabel";
        _endLabel.Size = new Size(69, 15);
        _endLabel.TabIndex = 2;
        _endLabel.Text = "Окончание";

        _dtpOtherEnd.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _dtpOtherEnd.Format = DateTimePickerFormat.Short;
        _dtpOtherEnd.Location = new Point(155, 60);
        _dtpOtherEnd.Name = "_dtpOtherEnd";
        _dtpOtherEnd.Size = new Size(264, 23);
        _dtpOtherEnd.TabIndex = 3;
        _dtpOtherEnd.Value = new DateTime(2026, 5, 7, 0, 0, 0, 0);

        _btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _btnOk.Location = new Point(196, 132);
        _btnOk.Name = "_btnOk";
        _btnOk.Size = new Size(108, 27);
        _btnOk.TabIndex = 4;
        _btnOk.Text = "Сравнить";
        _btnOk.UseVisualStyleBackColor = true;
        _btnOk.Click += OnOkClick;

        _btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _btnCancel.DialogResult = DialogResult.Cancel;
        _btnCancel.Location = new Point(310, 132);
        _btnCancel.Name = "_btnCancel";
        _btnCancel.Size = new Size(108, 27);
        _btnCancel.TabIndex = 5;
        _btnCancel.Text = "Отмена";
        _btnCancel.UseVisualStyleBackColor = true;

        AcceptButton = _btnOk;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = _btnCancel;
        ClientSize = new Size(444, 184);
        Controls.AddRange(new Control[]
        {
            _startLabel,
            _dtpOtherStart,
            _endLabel,
            _dtpOtherEnd,
            _btnOk,
            _btnCancel
        });
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(460, 223);
        Name = "ComparePeriodDialog";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Сравнение с другим периодом";
        ResumeLayout(false);
        PerformLayout();
    }
}
