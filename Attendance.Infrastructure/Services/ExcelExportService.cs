using Attendance.Infrastructure.DTOs;
using ClosedXML.Excel;
using Attendance.Infrastructure.Resources;
using Microsoft.Extensions.Localization;

namespace Attendance.Infrastructure.Services;

public interface IExcelExportService
{
    byte[] ExportStatisticsToExcel(CourseStatisticsDto statistics, string courseName);
}

public class ExcelExportService(IStringLocalizer<ExportResource> localizer) : IExcelExportService
{
    public byte[] ExportStatisticsToExcel(CourseStatisticsDto statistics, string courseName)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(localizer["Excel_Sheet_Attendance"]);

        // ── Header row ──────────────────────────────────────────────
        var headers = new[]
        {
            localizer["Excel_Header_Rank"].Value,
            localizer["Excel_Header_Student"].Value,
            localizer["Excel_Header_Present"].Value,
            localizer["Excel_Header_AbsentF"].Value,
            localizer["Excel_Header_LateR"].Value,
            localizer["Excel_Header_FJ"].Value,
            localizer["Excel_Header_RJ"].Value,
            localizer["Excel_Header_SAF"].Value,
            localizer["Excel_Header_SAFJ"].Value,
            localizer["Excel_Header_AttendancePct"].Value,
            localizer["Excel_Header_DiplomaEligible"].Value,
            localizer["Excel_Header_AtRisk"].Value
        };

        for (var i = 0; i < headers.Length; i++)
        {
            var cell = sheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2C5F8A");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // ── Data rows ────────────────────────────────────────────────
        var row = 2;
        foreach (var student in statistics.Students)
        {
            sheet.Cell(row, 1).Value = student.Rank;
            sheet.Cell(row, 2).Value = student.FullName;
            sheet.Cell(row, 3).Value = student.PresentCount;
            sheet.Cell(row, 4).Value = student.AbsentCount;
            sheet.Cell(row, 5).Value = student.LateCount;
            sheet.Cell(row, 6).Value = student.JustifiedAbsentCount;
            sheet.Cell(row, 7).Value = student.JustifiedLateCount;
            sheet.Cell(row, 8).Value = student.EarlyLeaveCount;
            sheet.Cell(row, 9).Value = student.JustifiedEarlyLeaveCount;
            sheet.Cell(row, 10).Value = (double)student.RealAttendancePercentage;
            sheet.Cell(row, 10).Style.NumberFormat.Format = "0.0\"%\"";
            sheet.Cell(row, 11).Value = student.DiplomaEligible ? "✓" : "✗";
            sheet.Cell(row, 12).Value = student.AtRiskDrop ? "⚠" : "";

            // Color row by risk
            if (student.AtRiskDrop)
            {
                sheet.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#FDECEA");
            }
            else if (!student.DiplomaEligible)
            {
                sheet.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFF3E0");
            }

            row++;
        }

        // ── Summary row ──────────────────────────────────────────────
        sheet.Cell(row, 1).Value = localizer["Excel_Summary_Average"].Value;
        sheet.Cell(row, 1).Style.Font.Bold = true;
        sheet.Cell(row, 10).Value = (double)statistics.AverageRealAttendancePercentage;
        sheet.Cell(row, 10).Style.NumberFormat.Format = "0.0\"%\"";
        sheet.Cell(row, 10).Style.Font.Bold = true;

        // ── Auto-fit columns ─────────────────────────────────────────
        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
