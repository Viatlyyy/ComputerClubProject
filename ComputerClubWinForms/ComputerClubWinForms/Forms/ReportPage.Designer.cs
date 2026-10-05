#nullable enable

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ComputerClubWinForms.Forms;

partial class ReportPage
{
    private Label _fromLabel = null!;
    private Label _toLabel = null!;
    private DateTimePicker _dtpFrom = null!;
    private DateTimePicker _dtpTo = null!;
    private Button _btnGenerate = null!;
    private Button _btnCompare = null!;
    private Button _btnExportPdf = null!;
    private DataGridView _dgvReport = null!;
    private Label _lblTotalRevenue = null!;

    private void InitializeComponent()
    {
        _fromLabel = new Label();
        _dtpFrom = new DateTimePicker();
        _toLabel = new Label();
        _dtpTo = new DateTimePicker();
        _btnGenerate = new Button();
        _btnCompare = new Button();
        _btnExportPdf = new Button();
        _dgvReport = new DataGridView();
        _lblTotalRevenue = new Label();
        ((System.ComponentModel.ISupportInitialize)_dgvReport).BeginInit();
        SuspendLayout();
        _fromLabel.AutoSize = true;
        _fromLabel.Location = new Point(12, 20);
        _fromLabel.Name = "_fromLabel";
        _fromLabel.Size = new Size(58, 15);
        _fromLabel.TabIndex = 0;
        _fromLabel.Text = "Период с";
        _dtpFrom.Format = DateTimePickerFormat.Short;
        _dtpFrom.Location = new Point(78, 16);
        _dtpFrom.Name = "_dtpFrom";
        _dtpFrom.Size = new Size(120, 23);
        _dtpFrom.TabIndex = 1;
        _dtpFrom.Value = new DateTime(2026, 5, 10, 0, 0, 0, 0);
        _toLabel.AutoSize = true;
        _toLabel.Location = new Point(214, 20);
        _toLabel.Name = "_toLabel";
        _toLabel.Size = new Size(21, 15);
        _toLabel.TabIndex = 2;
        _toLabel.Text = "по";
        _dtpTo.Format = DateTimePickerFormat.Short;
        _dtpTo.Location = new Point(246, 16);
        _dtpTo.Name = "_dtpTo";
        _dtpTo.Size = new Size(120, 23);
        _dtpTo.TabIndex = 3;
        _dtpTo.Value = new DateTime(2026, 5, 16, 0, 0, 0, 0);
        _btnGenerate.Location = new Point(382, 14);
        _btnGenerate.Name = "_btnGenerate";
        _btnGenerate.Size = new Size(118, 27);
        _btnGenerate.TabIndex = 4;
        _btnGenerate.Text = "Сформировать";
        _btnGenerate.UseVisualStyleBackColor = true;
        _btnGenerate.Click += OnGenerateClick;
        _btnCompare.Location = new Point(506, 14);
        _btnCompare.Name = "_btnCompare";
        _btnCompare.Size = new Size(92, 27);
        _btnCompare.TabIndex = 5;
        _btnCompare.Text = "Сравнить";
        _btnCompare.UseVisualStyleBackColor = true;
        _btnCompare.Click += OnCompareClick;
        _btnExportPdf.Location = new Point(604, 14);
        _btnExportPdf.Name = "_btnExportPdf";
        _btnExportPdf.Size = new Size(110, 27);
        _btnExportPdf.TabIndex = 6;
        _btnExportPdf.Text = "Экспорт в PDF";
        _btnExportPdf.UseVisualStyleBackColor = true;
        _btnExportPdf.Click += OnExportPdfClick;
        _dgvReport.AllowUserToAddRows = false;
        _dgvReport.AllowUserToDeleteRows = false;
        _dgvReport.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _dgvReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _dgvReport.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        _dgvReport.Location = new Point(12, 54);
        _dgvReport.Name = "_dgvReport";
        _dgvReport.ReadOnly = true;
        _dgvReport.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _dgvReport.Size = new Size(1532, 653);
        _dgvReport.TabIndex = 7;
        _lblTotalRevenue.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _lblTotalRevenue.AutoSize = true;
        _lblTotalRevenue.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        _lblTotalRevenue.Location = new Point(12, 723);
        _lblTotalRevenue.Name = "_lblTotalRevenue";
        _lblTotalRevenue.Size = new Size(158, 15);
        _lblTotalRevenue.TabIndex = 8;
        _lblTotalRevenue.Text = "Итого: 0 сеансов, 0,00 руб.";
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(_fromLabel);
        Controls.Add(_dtpFrom);
        Controls.Add(_toLabel);
        Controls.Add(_dtpTo);
        Controls.Add(_btnGenerate);
        Controls.Add(_btnCompare);
        Controls.Add(_btnExportPdf);
        Controls.Add(_dgvReport);
        Controls.Add(_lblTotalRevenue);
        Name = "ReportPage";
        Padding = new Padding(12);
        Size = new Size(1556, 755);
        ((System.ComponentModel.ISupportInitialize)_dgvReport).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
