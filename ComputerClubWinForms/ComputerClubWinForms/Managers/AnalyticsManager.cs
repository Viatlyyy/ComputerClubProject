using System;
using System.Collections.Generic;
using System.Linq;
using ComputerClubWinForms.Models;

namespace ComputerClubWinForms.Managers;

public class AnalyticsManager
{
    private readonly SessionManager _sessionManager;

    public string LastMessage { get; private set; } = string.Empty;

    public AnalyticsManager(SessionManager sessionManager)
    {
        _sessionManager = sessionManager;
    }

    public RevenueReport? GenerateReport(DateTime start, DateTime end)
    {
        var from = start.Date;
        var to = end.Date.AddDays(1).AddTicks(-1);

        if (from > to)
            return Fail("Начальная дата не может быть позже конечной");

        try
        {
            var sessions = _sessionManager.GetCompletedSessionsBetweenDates(from, to);
            if (sessions.Count == 0)
                return Fail("За указанный период нет данных");

            var rows = sessions
                .Where(s => s.EndTime.HasValue)
                .GroupBy(s => s.EndTime!.Value.Date)
                .OrderBy(g => g.Key)
                .Select(g => new ReportDay
                {
                    Date = g.Key,
                    SessionsCount = g.Count(),
                    Revenue = g.Sum(s => s.TotalCost)
                })
                .ToList();

            LastMessage = "Отчёт сформирован";
            return new RevenueReport
            {
                PeriodStart = from,
                PeriodEnd = to.Date,
                DailyBreakdown = rows,
                TotalRevenue = rows.Sum(r => r.Revenue),
                TotalSessions = rows.Sum(r => r.SessionsCount)
            };
        }
        catch (Exception ex)
        {
            return Fail($"Не удалось сформировать отчёт: {ex.Message}");
        }
    }

    private RevenueReport? Fail(string message)
    {
        LastMessage = message;
        return null;
    }
}
