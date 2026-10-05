#nullable enable

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ComputerClubWinForms.Forms;

partial class SessionPage
{
    private System.ComponentModel.IContainer? components = null;
    private Label _lblClient = null!;
    private ComboBox _cmbClients = null!;
    private Label _lblComputer = null!;
    private ComboBox _cmbComputers = null!;
    private Label _lblDuration = null!;
    private NumericUpDown _numDurationMinutes = null!;
    private Button _btnStartDirect = null!;
    private Button _btnNewBooking = null!;
    private GroupBox _activeGroup = null!;
    private DataGridView _gridActiveSessions = null!;
    private Button _btnExtend = null!;
    private Button _btnFinish = null!;
    private GroupBox _bookingGroup = null!;
    private DataGridView _gridBookings = null!;
    private Button _btnStartBooking = null!;
    private Button _btnCancelBooking = null!;
    private GroupBox _mapGroup = null!;
    private Panel _computerMap = null!;
    private System.Windows.Forms.Timer _timer = null!;
    private DataGridViewTextBoxColumn _activeColId = null!;
    private DataGridViewTextBoxColumn _activeColClient = null!;
    private DataGridViewTextBoxColumn _activeColComputer = null!;
    private DataGridViewTextBoxColumn _activeColStart = null!;
    private DataGridViewTextBoxColumn _activeColPlannedEnd = null!;
    private DataGridViewTextBoxColumn _activeColRemaining = null!;
    private DataGridViewCheckBoxColumn _activeColIsExpired = null!;
    private DataGridViewTextBoxColumn _bookingColId = null!;
    private DataGridViewTextBoxColumn _bookingColClient = null!;
    private DataGridViewTextBoxColumn _bookingColComputer = null!;
    private DataGridViewTextBoxColumn _bookingColStart = null!;
    private DataGridViewTextBoxColumn _bookingColEnd = null!;
    private DataGridViewTextBoxColumn _bookingColDuration = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _lblClient = new Label();
        _cmbClients = new ComboBox();
        _lblComputer = new Label();
        _cmbComputers = new ComboBox();
        _lblDuration = new Label();
        _numDurationMinutes = new NumericUpDown();
        _btnStartDirect = new Button();
        _btnNewBooking = new Button();
        _activeGroup = new GroupBox();
        _gridActiveSessions = new DataGridView();
        _activeColId = new DataGridViewTextBoxColumn();
        _activeColClient = new DataGridViewTextBoxColumn();
        _activeColComputer = new DataGridViewTextBoxColumn();
        _activeColStart = new DataGridViewTextBoxColumn();
        _activeColPlannedEnd = new DataGridViewTextBoxColumn();
        _activeColRemaining = new DataGridViewTextBoxColumn();
        _activeColIsExpired = new DataGridViewCheckBoxColumn();
        _btnExtend = new Button();
        _btnFinish = new Button();
        _bookingGroup = new GroupBox();
        _gridBookings = new DataGridView();
        _bookingColId = new DataGridViewTextBoxColumn();
        _bookingColClient = new DataGridViewTextBoxColumn();
        _bookingColComputer = new DataGridViewTextBoxColumn();
        _bookingColStart = new DataGridViewTextBoxColumn();
        _bookingColEnd = new DataGridViewTextBoxColumn();
        _bookingColDuration = new DataGridViewTextBoxColumn();
        _btnStartBooking = new Button();
        _btnCancelBooking = new Button();
        _mapGroup = new GroupBox();
        _computerMap = new Panel();
        _timer = new System.Windows.Forms.Timer(components);
        ((System.ComponentModel.ISupportInitialize)_numDurationMinutes).BeginInit();
        _activeGroup.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_gridActiveSessions).BeginInit();
        _bookingGroup.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_gridBookings).BeginInit();
        _mapGroup.SuspendLayout();
        SuspendLayout();

        _lblClient.AutoSize = true;
        _lblClient.Location = new Point(12, 20);
        _lblClient.Name = "_lblClient";
        _lblClient.Size = new Size(48, 15);
        _lblClient.TabIndex = 0;
        _lblClient.Text = "Клиент";

        _cmbClients.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbClients.FormattingEnabled = true;
        _cmbClients.Location = new Point(68, 16);
        _cmbClients.Name = "_cmbClients";
        _cmbClients.Size = new Size(240, 23);
        _cmbClients.TabIndex = 1;

        _lblComputer.AutoSize = true;
        _lblComputer.Location = new Point(320, 20);
        _lblComputer.Name = "_lblComputer";
        _lblComputer.Size = new Size(73, 15);
        _lblComputer.TabIndex = 2;
        _lblComputer.Text = "Компьютер";

        _cmbComputers.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbComputers.FormattingEnabled = true;
        _cmbComputers.Location = new Point(398, 16);
        _cmbComputers.Name = "_cmbComputers";
        _cmbComputers.Size = new Size(90, 23);
        _cmbComputers.TabIndex = 3;

        _lblDuration.AutoSize = true;
        _lblDuration.Location = new Point(502, 20);
        _lblDuration.Name = "_lblDuration";
        _lblDuration.Size = new Size(112, 15);
        _lblDuration.TabIndex = 4;
        _lblDuration.Text = "Длительность, мин";

        _numDurationMinutes.Location = new Point(622, 16);
        _numDurationMinutes.Maximum = new decimal(new int[] { 1440, 0, 0, 0 });
        _numDurationMinutes.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        _numDurationMinutes.Name = "_numDurationMinutes";
        _numDurationMinutes.Size = new Size(70, 23);
        _numDurationMinutes.TabIndex = 5;
        _numDurationMinutes.Value = new decimal(new int[] { 60, 0, 0, 0 });

        _btnStartDirect.Location = new Point(708, 14);
        _btnStartDirect.Name = "_btnStartDirect";
        _btnStartDirect.Size = new Size(128, 27);
        _btnStartDirect.TabIndex = 6;
        _btnStartDirect.Text = "Начать напрямую";
        _btnStartDirect.UseVisualStyleBackColor = true;
        _btnStartDirect.Click += OnStartDirectClick;

        _btnNewBooking.Location = new Point(842, 14);
        _btnNewBooking.Name = "_btnNewBooking";
        _btnNewBooking.Size = new Size(110, 27);
        _btnNewBooking.TabIndex = 7;
        _btnNewBooking.Text = "Новая бронь";
        _btnNewBooking.UseVisualStyleBackColor = true;
        _btnNewBooking.Click += OnCreateBookingClick;

        _activeGroup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _activeGroup.Controls.Add(_gridActiveSessions);
        _activeGroup.Controls.Add(_btnExtend);
        _activeGroup.Controls.Add(_btnFinish);
        _activeGroup.Location = new Point(12, 54);
        _activeGroup.Name = "_activeGroup";
        _activeGroup.Size = new Size(976, 245);
        _activeGroup.TabIndex = 8;
        _activeGroup.TabStop = false;
        _activeGroup.Text = "Активные сеансы";

        _gridActiveSessions.AllowUserToAddRows = false;
        _gridActiveSessions.AllowUserToDeleteRows = false;
        _gridActiveSessions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _gridActiveSessions.AutoGenerateColumns = false;
        _gridActiveSessions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _gridActiveSessions.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        _gridActiveSessions.Columns.AddRange(new DataGridViewColumn[] { _activeColId, _activeColClient, _activeColComputer, _activeColStart, _activeColPlannedEnd, _activeColRemaining, _activeColIsExpired });
        _gridActiveSessions.Location = new Point(12, 24);
        _gridActiveSessions.MultiSelect = false;
        _gridActiveSessions.Name = "_gridActiveSessions";
        _gridActiveSessions.ReadOnly = true;
        _gridActiveSessions.RowTemplate.Height = 25;
        _gridActiveSessions.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _gridActiveSessions.Size = new Size(952, 176);
        _gridActiveSessions.TabIndex = 0;
        _gridActiveSessions.CellFormatting += ActiveSessionsCellFormatting;

        _activeColId.DataPropertyName = "Id";
        _activeColId.HeaderText = "ID";
        _activeColId.Name = "_activeColId";
        _activeColId.ReadOnly = true;

        _activeColClient.DataPropertyName = "Client";
        _activeColClient.HeaderText = "Клиент";
        _activeColClient.Name = "_activeColClient";
        _activeColClient.ReadOnly = true;

        _activeColComputer.DataPropertyName = "Computer";
        _activeColComputer.HeaderText = "Компьютер";
        _activeColComputer.Name = "_activeColComputer";
        _activeColComputer.ReadOnly = true;

        _activeColStart.DataPropertyName = "StartTime";
        _activeColStart.HeaderText = "Начало";
        _activeColStart.Name = "_activeColStart";
        _activeColStart.ReadOnly = true;

        _activeColPlannedEnd.DataPropertyName = "PlannedEndTime";
        _activeColPlannedEnd.HeaderText = "План. окончание";
        _activeColPlannedEnd.Name = "_activeColPlannedEnd";
        _activeColPlannedEnd.ReadOnly = true;

        _activeColRemaining.DataPropertyName = "Remaining";
        _activeColRemaining.HeaderText = "Осталось";
        _activeColRemaining.Name = "_activeColRemaining";
        _activeColRemaining.ReadOnly = true;

        _activeColIsExpired.DataPropertyName = "IsExpired";
        _activeColIsExpired.HeaderText = "Время вышло";
        _activeColIsExpired.Name = "_activeColIsExpired";
        _activeColIsExpired.ReadOnly = true;

        _btnExtend.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _btnExtend.Location = new Point(12, 208);
        _btnExtend.Name = "_btnExtend";
        _btnExtend.Size = new Size(90, 27);
        _btnExtend.TabIndex = 1;
        _btnExtend.Text = "Продлить";
        _btnExtend.UseVisualStyleBackColor = true;
        _btnExtend.Click += OnExtendSessionClick;

        _btnFinish.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _btnFinish.Location = new Point(108, 208);
        _btnFinish.Name = "_btnFinish";
        _btnFinish.Size = new Size(94, 27);
        _btnFinish.TabIndex = 2;
        _btnFinish.Text = "Завершить";
        _btnFinish.UseVisualStyleBackColor = true;
        _btnFinish.Click += OnFinishSessionClick;

        _bookingGroup.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _bookingGroup.Controls.Add(_gridBookings);
        _bookingGroup.Controls.Add(_btnStartBooking);
        _bookingGroup.Controls.Add(_btnCancelBooking);
        _bookingGroup.Location = new Point(12, 308);
        _bookingGroup.Name = "_bookingGroup";
        _bookingGroup.Size = new Size(976, 205);
        _bookingGroup.TabIndex = 9;
        _bookingGroup.TabStop = false;
        _bookingGroup.Text = "Брони";

        _gridBookings.AllowUserToAddRows = false;
        _gridBookings.AllowUserToDeleteRows = false;
        _gridBookings.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _gridBookings.AutoGenerateColumns = false;
        _gridBookings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _gridBookings.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        _gridBookings.Columns.AddRange(new DataGridViewColumn[] { _bookingColId, _bookingColClient, _bookingColComputer, _bookingColStart, _bookingColEnd, _bookingColDuration });
        _gridBookings.Location = new Point(12, 24);
        _gridBookings.MultiSelect = false;
        _gridBookings.Name = "_gridBookings";
        _gridBookings.ReadOnly = true;
        _gridBookings.RowTemplate.Height = 25;
        _gridBookings.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _gridBookings.Size = new Size(952, 128);
        _gridBookings.TabIndex = 0;

        _bookingColId.DataPropertyName = "Id";
        _bookingColId.HeaderText = "ID";
        _bookingColId.Name = "_bookingColId";
        _bookingColId.ReadOnly = true;

        _bookingColClient.DataPropertyName = "Client";
        _bookingColClient.HeaderText = "Клиент";
        _bookingColClient.Name = "_bookingColClient";
        _bookingColClient.ReadOnly = true;

        _bookingColComputer.DataPropertyName = "Computer";
        _bookingColComputer.HeaderText = "Компьютер";
        _bookingColComputer.Name = "_bookingColComputer";
        _bookingColComputer.ReadOnly = true;

        _bookingColStart.DataPropertyName = "Start";
        _bookingColStart.HeaderText = "Начало";
        _bookingColStart.Name = "_bookingColStart";
        _bookingColStart.ReadOnly = true;

        _bookingColEnd.DataPropertyName = "End";
        _bookingColEnd.HeaderText = "Окончание";
        _bookingColEnd.Name = "_bookingColEnd";
        _bookingColEnd.ReadOnly = true;

        _bookingColDuration.DataPropertyName = "DurationMinutes";
        _bookingColDuration.HeaderText = "Минут";
        _bookingColDuration.Name = "_bookingColDuration";
        _bookingColDuration.ReadOnly = true;

        _btnStartBooking.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _btnStartBooking.Location = new Point(12, 164);
        _btnStartBooking.Name = "_btnStartBooking";
        _btnStartBooking.Size = new Size(122, 27);
        _btnStartBooking.TabIndex = 1;
        _btnStartBooking.Text = "Начать по брони";
        _btnStartBooking.UseVisualStyleBackColor = true;
        _btnStartBooking.Click += OnStartBookingClick;

        _btnCancelBooking.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _btnCancelBooking.Location = new Point(140, 164);
        _btnCancelBooking.Name = "_btnCancelBooking";
        _btnCancelBooking.Size = new Size(124, 27);
        _btnCancelBooking.TabIndex = 2;
        _btnCancelBooking.Text = "Отменить бронь";
        _btnCancelBooking.UseVisualStyleBackColor = true;
        _btnCancelBooking.Click += OnCancelBookingClick;

        _mapGroup.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _mapGroup.Controls.Add(_computerMap);
        _mapGroup.Location = new Point(12, 522);
        _mapGroup.Name = "_mapGroup";
        _mapGroup.Size = new Size(976, 116);
        _mapGroup.TabIndex = 10;
        _mapGroup.TabStop = false;
        _mapGroup.Text = "Схема зала: красный — занят/забронирован, зелёный — свободен";

        _computerMap.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _computerMap.AutoScroll = true;
        _computerMap.BorderStyle = BorderStyle.FixedSingle;
        _computerMap.Location = new Point(12, 24);
        _computerMap.Name = "_computerMap";
        _computerMap.Size = new Size(952, 80);
        _computerMap.TabIndex = 0;

        _timer.Interval = 1000;
        _timer.Tick += SessionTimer_Tick;

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.AddRange(new Control[]
        {
            _lblClient,
            _cmbClients,
            _lblComputer,
            _cmbComputers,
            _lblDuration,
            _numDurationMinutes,
            _btnStartDirect,
            _btnNewBooking,
            _activeGroup,
            _bookingGroup,
            _mapGroup
        });
        Name = "SessionPage";
        Padding = new Padding(12);
        Size = new Size(1000, 650);
        ((System.ComponentModel.ISupportInitialize)_numDurationMinutes).EndInit();
        _activeGroup.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_gridActiveSessions).EndInit();
        _bookingGroup.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_gridBookings).EndInit();
        _mapGroup.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }
}
