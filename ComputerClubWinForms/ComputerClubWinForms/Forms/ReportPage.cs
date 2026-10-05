using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using ComputerClubWinForms.Managers;
using ComputerClubWinForms.Models;

namespace ComputerClubWinForms.Forms;

public partial class ReportPage : UserControl
{
    private AnalyticsManager? _analyticsManager;
    private RevenueReport? _currentReport;

    public ReportPage()
    {
        InitializeComponent();

        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            ShowDesignPreviewReport();
    }

    public ReportPage(AnalyticsManager analyticsManager) : this()
    {
        _analyticsManager = analyticsManager;
        _dtpFrom.Value = DateTime.Now.Date.AddDays(-7);
        _dtpTo.Value = DateTime.Now.Date;
        _dgvReport.DataSource = null;
        _lblTotalRevenue.Text = "Выберите период и нажмите «Сформировать».";
    }

    private void ShowDesignPreviewReport()
    {
        _dgvReport.DataSource = new List<ReportRow>
        {
            new ReportRow { Date = DateTime.Today.AddDays(-2).ToString("dd.MM.yyyy"), SessionsCount = 15, Revenue = 4500m },
            new ReportRow { Date = DateTime.Today.AddDays(-1).ToString("dd.MM.yyyy"), SessionsCount = 22, Revenue = 6600m },
            new ReportRow { Date = "Итого", SessionsCount = 37, Revenue = 11100m }
        };
        _lblTotalRevenue.Text = "Итого: 37 сеансов, 11 100,00 руб. (пример конструктора)";
    }

    private void OnGenerateClick(object? sender, EventArgs e)
    {
        if (_analyticsManager is null)
            return;

        var report = _analyticsManager.GenerateReport(_dtpFrom.Value, _dtpTo.Value);
        if (report is null)
        {
            _currentReport = null;
            _dgvReport.DataSource = null;
            _lblTotalRevenue.Text = _analyticsManager.LastMessage;
            MessageBox.Show(_analyticsManager.LastMessage, "Отчёт", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _currentReport = report;
        ShowReport(report);
    }

    public void ShowReport(RevenueReport report)
    {
        var culture = CultureInfo.GetCultureInfo("ru-RU");
        var rows = report.DailyBreakdown.Select(d => new ReportRow
        {
            Date = d.Date.ToString("dd.MM.yyyy"),
            SessionsCount = d.SessionsCount,
            Revenue = d.Revenue
        }).ToList();

        rows.Add(new ReportRow
        {
            Date = "Итого",
            SessionsCount = report.TotalSessions,
            Revenue = report.TotalRevenue
        });

        _dgvReport.DataSource = rows;
        if (_dgvReport.Columns[nameof(ReportRow.Date)] is DataGridViewColumn dateColumn) dateColumn.HeaderText = "Дата";
        if (_dgvReport.Columns[nameof(ReportRow.SessionsCount)] is DataGridViewColumn countColumn) countColumn.HeaderText = "Количество сеансов";
        if (_dgvReport.Columns[nameof(ReportRow.Revenue)] is DataGridViewColumn revenueColumn)
        {
            revenueColumn.HeaderText = "Выручка, руб.";
            revenueColumn.DefaultCellStyle.Format = "N2";
        }

        _lblTotalRevenue.Text = $"Итого: {report.TotalSessions} сеансов, {report.TotalRevenue.ToString("N2", culture)} руб.";
    }

    private void OnCompareClick(object? sender, EventArgs e)
    {
        if (_analyticsManager is null)
            return;

        if (_currentReport is null)
        {
            var report = _analyticsManager.GenerateReport(_dtpFrom.Value, _dtpTo.Value);
            if (report is null)
            {
                MessageBox.Show(_analyticsManager.LastMessage, "Сравнение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            _currentReport = report;
            ShowReport(_currentReport);
        }

        using var dialog = new ComparePeriodDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        var otherReport = _analyticsManager.GenerateReport(dialog.OtherStart, dialog.OtherEnd);
        if (otherReport is null)
        {
            MessageBox.Show(_analyticsManager.LastMessage, "Сравнение", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var revenueDifference = _currentReport.GetRevenueDifference(otherReport);
        var percentChange = _currentReport.GetRevenuePercentChange(otherReport);
        var sessionsDifference = otherReport.TotalSessions - _currentReport.TotalSessions;
        var culture = CultureInfo.GetCultureInfo("ru-RU");
        var message = $"Первый период: {_currentReport.TotalRevenue.ToString("N2", culture)} руб., {_currentReport.TotalSessions} сеансов\n" +
                      $"Второй период: {otherReport.TotalRevenue.ToString("N2", culture)} руб., {otherReport.TotalSessions} сеансов\n" +
                      $"Изменение выручки: {revenueDifference.ToString("N2", culture)} руб. ({percentChange.ToString("N2", culture)}%)\n" +
                      $"Изменение количества сеансов: {sessionsDifference}";
        MessageBox.Show(message, "Сравнение периодов", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void OnExportPdfClick(object? sender, EventArgs e)
    {
        if (_currentReport is null)
        {
            MessageBox.Show("Сначала сформируйте отчёт", "Экспорт", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new SaveFileDialog();
        dialog.Filter = "PDF-файл (*.pdf)|*.pdf";
        dialog.FileName = $"report_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            _currentReport.ExportToPdf(dialog.FileName);
            MessageBox.Show("PDF сохранён", "Экспорт", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось сохранить PDF: {ex.Message}", "Экспорт", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private class ReportRow
    {
        public string Date { get; set; } = string.Empty;
        public int SessionsCount { get; set; }
        public decimal Revenue { get; set; }
    }
}
