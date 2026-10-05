#nullable enable

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ComputerClubWinForms.Forms;

partial class BookingDialog
{
    private Label _dateLabel = null!;
    private Label _timeLabel = null!;
    private Label _durationLabel = null!;
    private Label _computerLabel = null!;
    private Label _mapLabel = null!;
    private DateTimePicker _dtpDate = null!;
    private DateTimePicker _dtpTime = null!;
    private NumericUpDown _numDurationMinutes = null!;
    private ComboBox _cmbComputers = null!;
    private Panel _computerMap = null!;
    private Button _btnOk = null!;
    private Button _btnCancel = null!;

    private void InitializeComponent()
    {
        _dateLabel = new Label();
        _timeLabel = new Label();
        _durationLabel = new Label();
        _computerLabel = new Label();
        _mapLabel = new Label();
        _dtpDate = new DateTimePicker();
        _dtpTime = new DateTimePicker();
        _numDurationMinutes = new NumericUpDown();
        _cmbComputers = new ComboBox();
        _computerMap = new Panel();
        _btnOk = new Button();
        _btnCancel = new Button();
        ((System.ComponentModel.ISupportInitialize)_numDurationMinutes).BeginInit();
        SuspendLayout();

        _dateLabel.AutoSize = true;
        _dateLabel.Location = new Point(24, 26);
        _dateLabel.Name = "_dateLabel";
        _dateLabel.Size = new Size(32, 15);
        _dateLabel.TabIndex = 0;
        _dateLabel.Text = "Дата";

        _dtpDate.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _dtpDate.Format = DateTimePickerFormat.Short;
        _dtpDate.Location = new Point(176, 22);
        _dtpDate.Name = "_dtpDate";
        _dtpDate.Size = new Size(382, 23);
        _dtpDate.TabIndex = 1;
        _dtpDate.Value = new DateTime(2026, 5, 15, 0, 0, 0, 0);
        _dtpDate.ValueChanged += DateOrDurationChanged;

        _timeLabel.AutoSize = true;
        _timeLabel.Location = new Point(24, 60);
        _timeLabel.Name = "_timeLabel";
        _timeLabel.Size = new Size(81, 15);
        _timeLabel.TabIndex = 2;
        _timeLabel.Text = "Время начала";

        _dtpTime.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _dtpTime.CustomFormat = "HH:mm";
        _dtpTime.Format = DateTimePickerFormat.Custom;
        _dtpTime.Location = new Point(176, 56);
        _dtpTime.Name = "_dtpTime";
        _dtpTime.ShowUpDown = true;
        _dtpTime.Size = new Size(382, 23);
        _dtpTime.TabIndex = 3;
        _dtpTime.Value = new DateTime(2026, 5, 15, 12, 0, 0, 0);
        _dtpTime.ValueChanged += DateOrDurationChanged;

        _durationLabel.AutoSize = true;
        _durationLabel.Location = new Point(24, 94);
        _durationLabel.Name = "_durationLabel";
        _durationLabel.Size = new Size(108, 15);
        _durationLabel.TabIndex = 4;
        _durationLabel.Text = "Длительность, мин";

        _numDurationMinutes.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _numDurationMinutes.Location = new Point(176, 90);
        _numDurationMinutes.Maximum = new decimal(new int[] { 1440, 0, 0, 0 });
        _numDurationMinutes.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        _numDurationMinutes.Name = "_numDurationMinutes";
        _numDurationMinutes.Size = new Size(382, 23);
        _numDurationMinutes.TabIndex = 5;
        _numDurationMinutes.Value = new decimal(new int[] { 60, 0, 0, 0 });
        _numDurationMinutes.ValueChanged += DateOrDurationChanged;

        _computerLabel.AutoSize = true;
        _computerLabel.Location = new Point(24, 128);
        _computerLabel.Name = "_computerLabel";
        _computerLabel.Size = new Size(73, 15);
        _computerLabel.TabIndex = 6;
        _computerLabel.Text = "Компьютер";

        _cmbComputers.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _cmbComputers.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbComputers.FormattingEnabled = true;
        _cmbComputers.Location = new Point(176, 124);
        _cmbComputers.Name = "_cmbComputers";
        _cmbComputers.Size = new Size(382, 23);
        _cmbComputers.TabIndex = 7;
        _cmbComputers.SelectedValueChanged += ComputerSelectionChanged;

        _mapLabel.AutoSize = true;
        _mapLabel.Location = new Point(24, 164);
        _mapLabel.Name = "_mapLabel";
        _mapLabel.Size = new Size(67, 15);
        _mapLabel.TabIndex = 8;
        _mapLabel.Text = "Схема зала";

        _computerMap.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _computerMap.AutoScroll = true;
        _computerMap.BorderStyle = BorderStyle.FixedSingle;
        _computerMap.Location = new Point(176, 160);
        _computerMap.Name = "_computerMap";
        _computerMap.Size = new Size(382, 262);
        _computerMap.TabIndex = 9;

        _btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _btnOk.Location = new Point(332, 444);
        _btnOk.Name = "_btnOk";
        _btnOk.Size = new Size(110, 27);
        _btnOk.TabIndex = 10;
        _btnOk.Text = "Создать";
        _btnOk.UseVisualStyleBackColor = true;
        _btnOk.Click += OnOkClick;

        _btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _btnCancel.DialogResult = DialogResult.Cancel;
        _btnCancel.Location = new Point(448, 444);
        _btnCancel.Name = "_btnCancel";
        _btnCancel.Size = new Size(110, 27);
        _btnCancel.TabIndex = 11;
        _btnCancel.Text = "Отмена";
        _btnCancel.UseVisualStyleBackColor = true;

        AcceptButton = _btnOk;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = _btnCancel;
        ClientSize = new Size(582, 490);
        Controls.AddRange(new Control[]
        {
            _dateLabel,
            _dtpDate,
            _timeLabel,
            _dtpTime,
            _durationLabel,
            _numDurationMinutes,
            _computerLabel,
            _cmbComputers,
            _mapLabel,
            _computerMap,
            _btnOk,
            _btnCancel
        });
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(598, 529);
        Name = "BookingDialog";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Новая бронь";
        ((System.ComponentModel.ISupportInitialize)_numDurationMinutes).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
