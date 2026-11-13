#nullable enable

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ComputerClubWinForms.Forms;

partial class ClientPage
{
    private System.ComponentModel.IContainer? components = null;
    private Label _titleLabel = null!;
    private Label _searchLabel = null!;
    private TextBox _txtSearch = null!;
    private Button _btnAddClient = null!;
    private Button _btnEditClient = null!;
    private Button _btnDeleteClient = null!;
    private DataGridView _gridClients = null!;
    private DataGridViewTextBoxColumn _colId = null!;
    private DataGridViewTextBoxColumn _colFullName = null!;
    private DataGridViewTextBoxColumn _colPhone = null!;
    private DataGridViewTextBoxColumn _colTotalHours = null!;
    private DataGridViewTextBoxColumn _colTotalSpent = null!;
    private DataGridViewTextBoxColumn _colDiscount = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _titleLabel = new Label();
        _searchLabel = new Label();
        _txtSearch = new TextBox();
        _btnAddClient = new Button();
        _btnEditClient = new Button();
        _btnDeleteClient = new Button();
        _gridClients = new DataGridView();
        ((System.ComponentModel.ISupportInitialize)_gridClients).BeginInit();
        SuspendLayout();
        _titleLabel.AutoSize = true;
        _titleLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        _titleLabel.Location = new Point(24, 20);
        _titleLabel.Name = "_titleLabel";
        _titleLabel.Size = new Size(59, 15);
        _titleLabel.TabIndex = 0;
        _titleLabel.Text = "Клиенты";
        _searchLabel.AutoSize = true;
        _searchLabel.Location = new Point(24, 72);
        _searchLabel.Name = "_searchLabel";
        _searchLabel.TabIndex = 1;
        _searchLabel.Text = "Поиск";
        _txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _txtSearch.Location = new Point(88, 66);
        _txtSearch.Name = "_txtSearch";
        _txtSearch.PlaceholderText = "Поиск по ФИО или телефону";
        _txtSearch.Size = new Size(888, 27);
        _txtSearch.TabIndex = 2;
        _txtSearch.TextChanged += OnSearchTextChanged;
        _btnAddClient.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        _btnAddClient.Location = new Point(24, 112);
        _btnAddClient.Name = "_btnAddClient";
        _btnAddClient.Size = new Size(180, 44);
        _btnAddClient.TabIndex = 3;
        _btnAddClient.Text = "Добавить клиента";
        _btnAddClient.UseVisualStyleBackColor = true;
        _btnAddClient.Click += OnAddClientClick;
        _btnEditClient.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        _btnEditClient.Location = new Point(216, 112);
        _btnEditClient.Name = "_btnEditClient";
        _btnEditClient.Size = new Size(160, 44);
        _btnEditClient.TabIndex = 4;
        _btnEditClient.Text = "Редактировать";
        _btnEditClient.UseVisualStyleBackColor = true;
        _btnEditClient.Click += OnEditClientClick;
        _btnDeleteClient.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _btnDeleteClient.Location = new Point(846, 112);
        _btnDeleteClient.Name = "_btnDeleteClient";
        _btnDeleteClient.Size = new Size(130, 44);
        _btnDeleteClient.TabIndex = 5;
        _btnDeleteClient.Text = "Удалить";
        _btnDeleteClient.UseVisualStyleBackColor = true;
        _btnDeleteClient.Click += OnDeleteClientClick;
        _gridClients.AllowUserToAddRows = false;
        _gridClients.AllowUserToDeleteRows = false;
        _gridClients.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _gridClients.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _gridClients.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        _gridClients.Location = new Point(24, 174);
        _gridClients.MultiSelect = false;
        _gridClients.Name = "_gridClients";
        _gridClients.ReadOnly = true;
        _gridClients.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _gridClients.Size = new Size(952, 448);
        _gridClients.TabIndex = 6;
        _gridClients.CellDoubleClick += OnGridClientsCellDoubleClick;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Font = new Font("Segoe UI", 9F);
        Controls.Add(_titleLabel);
        Controls.Add(_searchLabel);
        Controls.Add(_txtSearch);
        Controls.Add(_btnAddClient);
        Controls.Add(_btnEditClient);
        Controls.Add(_btnDeleteClient);
        Controls.Add(_gridClients);
        Name = "ClientPage";
        Padding = new Padding(24);
        Size = new Size(1000, 646);
        ((System.ComponentModel.ISupportInitialize)_gridClients).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
