using System;
using System.Collections.Generic;
using System.Linq;
using ComputerClubWinForms.Utils;

namespace ComputerClubWinForms.Models;

public class RevenueReport
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public List<ReportDay> DailyBreakdown { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public int TotalSessions { get; set; }

    public decimal GetRevenueDifference(RevenueReport other)
    {
        return other.TotalRevenue - TotalRevenue;
    }

    public decimal GetRevenuePercentChange(RevenueReport other)
    {
        if (TotalRevenue == 0m && other.TotalRevenue == 0m)
            return 0m;
        if (TotalRevenue == 0m)
            return 100m;

        return Math.Round((other.TotalRevenue - TotalRevenue) / TotalRevenue * 100m, 2);
    }

    public bool ExportToPdf(string filePath)
    {
        PdfExporter.ExportRevenueReport(this, filePath);
        return true;
    }
}
