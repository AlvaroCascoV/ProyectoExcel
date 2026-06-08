# SKILL: excel-export
Use this skill when implementing the Excel export feature for statistics.

## When to trigger
- "Export statistics to Excel"
- "Add Excel download button"
- "Implement ClosedXML export"

## Package — add to Infrastructure only
```xml
<!-- Attendance.Infrastructure/Attendance.Infrastructure.csproj -->
<PackageReference Include="ClosedXML" Version="0.104.2" />
```

## Service pattern — ExcelExportService

```csharp
// Attendance.Infrastructure/Services/ExcelExportService.cs
using Attendance.Infrastructure.DTOs;
using ClosedXML.Excel;

namespace Attendance.Infrastructure.Services;

public interface IExcelExportService
{
    byte[] ExportStatisticsToExcel(CourseStatisticsDto statistics, string courseName);
}

public class ExcelExportService : IExcelExportService
{
    public byte[] ExportStatisticsToExcel(CourseStatisticsDto statistics, string courseName)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Attendance");

        // ── Header row ──────────────────────────────────────────────
        var headers = new[]
        {
            "Rank", "Student", "Present", "Absent (F)", "Late (R)",
            "FJ", "RJ", "SAF", "SAFJ",
            "Attendance %", "Diploma Eligible", "At Risk"
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
        sheet.Cell(row, 1).Value = "AVERAGE";
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
```

## API endpoint — add to StatisticsController
```csharp
// New action in ApiProyectoExcel/Controllers/StatisticsController.cs
[HttpGet("course/{courseId:int}/export")]
public async Task<IActionResult> ExportToExcel(
    int courseId,
    [FromQuery] int? month,
    [FromQuery] int? year,
    [FromQuery] decimal? minPercent,
    [FromQuery] decimal? maxPercent,
    CancellationToken cancellationToken = default)
{
    var statistics = await statisticsService.GetCourseStatisticsAsync(
        courseId, null, null, month, year, minPercent, maxPercent, cancellationToken);

    if (statistics is null)
        return NotFound(new { message = $"Course {courseId} not found." });

    var bytes = excelExportService.ExportStatisticsToExcel(statistics, statistics.CourseName);
    var fileName = $"attendance_{statistics.CourseName.Replace(" ", "_")}_{DateTime.Today:yyyyMMdd}.xlsx";

    return File(bytes,
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        fileName);
}
```

## IAttendanceApiClient — add method
```csharp
// ProyectoExcel/Services/AttendanceApiClient.cs
Task<byte[]?> ExportStatisticsToExcelAsync(
    int courseId, int? month, int? year,
    decimal? minPercent, decimal? maxPercent,
    CancellationToken cancellationToken = default);

// Implementation:
public async Task<byte[]?> ExportStatisticsToExcelAsync(
    int courseId, int? month, int? year,
    decimal? minPercent, decimal? maxPercent,
    CancellationToken cancellationToken = default)
{
    var query = BuildStatisticsQuery(month, year, minPercent, maxPercent);
    var response = await httpClient.GetAsync(
        $"api/statistics/course/{courseId}/export{query}", cancellationToken);

    if (response.StatusCode == HttpStatusCode.NotFound) return null;
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsByteArrayAsync(cancellationToken);
}
```

## MVC action + Razor button
```csharp
// StatisticsController.cs — new action
[HttpGet]
public async Task<IActionResult> Export(
    int courseId, int? month, int? year,
    decimal? minPercent, decimal? maxPercent,
    CancellationToken cancellationToken)
{
    var bytes = await apiClient.ExportStatisticsToExcelAsync(
        courseId, month, year, minPercent, maxPercent, cancellationToken);

    if (bytes is null) return NotFound();

    var fileName = $"attendance_{courseId}_{DateTime.Today:yyyyMMdd}.xlsx";
    return File(bytes,
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        fileName);
}
```

```html
<!-- In Statistics/Index.cshtml — next to existing filters -->
<a asp-action="Export"
   asp-route-courseId="@Model.SelectedCourseId"
   asp-route-month="@Model.SelectedMonth"
   asp-route-year="@Model.SelectedYear"
   asp-route-minPercent="@Model.MinPercent"
   asp-route-maxPercent="@Model.MaxPercent"
   class="btn btn-outline-success">
    <i class="bi bi-file-earmark-excel"></i> Export Excel
</a>
```

## Registration
```csharp
services.AddSingleton<IExcelExportService, ExcelExportService>();
```
