using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ComputerClubWinForms.Models;

namespace ComputerClubWinForms.Utils;

public static class PdfExporter
{
    private const int BitmapWidth = 1240;
    private const int BitmapHeight = 1754;
    private const int PdfWidth = 595;
    private const int PdfHeight = 842;
    private const int RowsPerPage = 30;

    public static void ExportRevenueReport(RevenueReport report, string filePath)
    {
        if (report is null)
            throw new ArgumentNullException(nameof(report));

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Путь к PDF не указан", nameof(filePath));

        var pages = BuildReportPages(report);
        WriteImagesAsPdf(pages, filePath);
    }

    private static List<byte[]> BuildReportPages(RevenueReport report)
    {
        var result = new List<byte[]>();
        var rows = report.DailyBreakdown.OrderBy(r => r.Date).ToList();
        var pageCount = Math.Max(1, (int)Math.Ceiling(rows.Count / (double)RowsPerPage));
        var culture = CultureInfo.GetCultureInfo("ru-RU");

        for (var pageIndex = 0; pageIndex < pageCount; pageIndex++)
        {
            var pageRows = rows.Skip(pageIndex * RowsPerPage).Take(RowsPerPage).ToList();
            var isLastPage = pageIndex == pageCount - 1;

            using var bitmap = new Bitmap(BitmapWidth, BitmapHeight);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.White);
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            using var titleFont = new Font("Arial", 34, FontStyle.Bold);
            using var subtitleFont = new Font("Arial", 22, FontStyle.Regular);
            using var headerFont = new Font("Arial", 20, FontStyle.Bold);
            using var rowFont = new Font("Arial", 19, FontStyle.Regular);
            using var smallFont = new Font("Arial", 16, FontStyle.Regular);
            using var pen = new Pen(Color.Black, 2);
            using var brush = new SolidBrush(Color.Black);

            var y = 80;
            graphics.DrawString("Отчёт о выручке компьютерного клуба", titleFont, brush, 80, y);
            y += 60;
            graphics.DrawString($"Период: {report.PeriodStart:dd.MM.yyyy} — {report.PeriodEnd:dd.MM.yyyy}", subtitleFont, brush, 80, y);
            y += 45;
            graphics.DrawString($"Страница {pageIndex + 1} из {pageCount}", smallFont, brush, 80, y);
            y += 65;

            var xDate = 80;
            var xSessions = 430;
            var xRevenue = 760;
            var tableWidth = 1060;
            var rowHeight = 46;

            graphics.DrawRectangle(pen, xDate, y, tableWidth, rowHeight);
            graphics.DrawLine(pen, xSessions - 25, y, xSessions - 25, y + rowHeight);
            graphics.DrawLine(pen, xRevenue - 25, y, xRevenue - 25, y + rowHeight);
            graphics.DrawString("Дата", headerFont, brush, xDate + 12, y + 9);
            graphics.DrawString("Количество сеансов", headerFont, brush, xSessions, y + 9);
            graphics.DrawString("Выручка, руб.", headerFont, brush, xRevenue, y + 9);
            y += rowHeight;

            foreach (var row in pageRows)
            {
                graphics.DrawRectangle(pen, xDate, y, tableWidth, rowHeight);
                graphics.DrawLine(pen, xSessions - 25, y, xSessions - 25, y + rowHeight);
                graphics.DrawLine(pen, xRevenue - 25, y, xRevenue - 25, y + rowHeight);
                graphics.DrawString(row.Date.ToString("dd.MM.yyyy", culture), rowFont, brush, xDate + 12, y + 10);
                graphics.DrawString(row.SessionsCount.ToString(culture), rowFont, brush, xSessions, y + 10);
                graphics.DrawString(row.Revenue.ToString("N2", culture), rowFont, brush, xRevenue, y + 10);
                y += rowHeight;
            }

            if (isLastPage)
            {
                y += 40;
                graphics.DrawString($"Итого сеансов: {report.TotalSessions}", headerFont, brush, 80, y);
                y += 42;
                graphics.DrawString($"Итого выручка: {report.TotalRevenue.ToString("N2", culture)} руб.", headerFont, brush, 80, y);
            }

            y = BitmapHeight - 90;
            graphics.DrawString($"Файл сформирован: {DateTime.Now:dd.MM.yyyy HH:mm}", smallFont, brush, 80, y);

            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Jpeg);
            result.Add(ms.ToArray());
        }

        return result;
    }

    private static void WriteImagesAsPdf(IReadOnlyList<byte[]> imagePages, string filePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(filePath)) ?? ".");

        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        var offsets = new List<long> { 0 };

        void WriteAscii(string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            fs.Write(bytes, 0, bytes.Length);
        }

        void BeginObject(int objectNumber)
        {
            while (offsets.Count <= objectNumber)
                offsets.Add(0);
            offsets[objectNumber] = fs.Position;
            WriteAscii($"{objectNumber} 0 obj\n");
        }

        WriteAscii("%PDF-1.4\n%\u00E2\u00E3\u00CF\u00D3\n");

        BeginObject(1);
        WriteAscii("<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");

        var pageObjectIds = Enumerable.Range(0, imagePages.Count).Select(i => 3 + i * 3).ToList();
        BeginObject(2);
        WriteAscii($"<< /Type /Pages /Kids [{string.Join(" ", pageObjectIds.Select(id => id + " 0 R"))}] /Count {imagePages.Count} >>\nendobj\n");

        for (var i = 0; i < imagePages.Count; i++)
        {
            var pageObject = 3 + i * 3;
            var imageObject = pageObject + 1;
            var contentObject = pageObject + 2;
            var imageBytes = imagePages[i];
            var content = $"q\n{PdfWidth} 0 0 {PdfHeight} 0 0 cm\n/Im{i + 1} Do\nQ\n";

            BeginObject(pageObject);
            WriteAscii($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {PdfWidth} {PdfHeight}] /Resources << /XObject << /Im{i + 1} {imageObject} 0 R >> >> /Contents {contentObject} 0 R >>\nendobj\n");

            BeginObject(imageObject);
            WriteAscii($"<< /Type /XObject /Subtype /Image /Width {BitmapWidth} /Height {BitmapHeight} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length {imageBytes.Length} >>\nstream\n");
            fs.Write(imageBytes, 0, imageBytes.Length);
            WriteAscii("\nendstream\nendobj\n");

            var contentBytes = Encoding.ASCII.GetBytes(content);
            BeginObject(contentObject);
            WriteAscii($"<< /Length {contentBytes.Length} >>\nstream\n");
            fs.Write(contentBytes, 0, contentBytes.Length);
            WriteAscii("\nendstream\nendobj\n");
        }

        var xrefOffset = fs.Position;
        var objectCount = 2 + imagePages.Count * 3;
        WriteAscii($"xref\n0 {objectCount + 1}\n");
        WriteAscii("0000000000 65535 f \n");
        for (var i = 1; i <= objectCount; i++)
            WriteAscii($"{offsets[i]:D10} 00000 n \n");

        WriteAscii($"trailer\n<< /Size {objectCount + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF");
    }
}
