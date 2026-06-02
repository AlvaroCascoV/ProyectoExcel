using Attendance.Infrastructure.DTOs;
using Attendance.Infrastructure.Entities;
using Attendance.Infrastructure.Resources;
using Microsoft.Extensions.Localization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace Attendance.Infrastructure.Services;

public interface IPdfExportService
{
    byte[] GenerateCourseStatisticsPdf(CourseStatisticsDto statistics, int? month = null, int? year = null);
    byte[] GenerateAttendanceSessionPdf(AttendanceSessionDto session);
}

public class PdfExportService(IStringLocalizer<ExportResource> localizer) : IPdfExportService
{
    private static readonly string PrimaryColor = "#1a237e";
    private static readonly string AltRowBg = "#f5f5f5";
    private static readonly string GreenColor = "#2e7d32";
    private static readonly string RedColor = "#c62828";
    private static readonly string OrangeColor = "#e65100";

    static PdfExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateCourseStatisticsPdf(CourseStatisticsDto statistics, int? month = null, int? year = null)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.MarginHorizontal(30);
                page.MarginVertical(25);
                page.DefaultTextStyle(x => x.FontSize(8));

                page.Header().Element(header => ComposeHeader(header, localizer["Pdf_Title_Statistics"], statistics.CourseName));

                page.Content().Element(content =>
                {
                    content.PaddingVertical(8).Column(col =>
                    {
                        col.Spacing(8);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text(text =>
                            {
                                text.Span(localizer["Pdf_Label_Course"] + " ").Bold();
                                text.Span(statistics.CourseName);
                            });

                            if (month.HasValue || year.HasValue)
                            {
                                row.RelativeItem().Text(text =>
                                {
                                    text.Span(localizer["Pdf_Label_Period"] + " ").Bold();
                                    var parts = new List<string>();
if (month.HasValue)
{
    var monthValue = month.Value;
    var monthName = monthValue is >= 1 and <= 12
        ? CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthValue)
        : monthValue.ToString(CultureInfo.InvariantCulture);

    parts.Add(string.Format(CultureInfo.CurrentCulture, localizer["Pdf_Period_Month"].Value, monthName));
}

                                    if (year.HasValue)
                                    {
                                        parts.Add(string.Format(CultureInfo.CurrentCulture, localizer["Pdf_Period_Year"].Value, year.Value));
                                    }
                                    text.Span(string.Join(", ", parts));
                                });
                            }

                            row.RelativeItem().Text(text =>
                            {
                                text.Span(localizer["Pdf_Label_LectiveDays"] + " ").Bold();
                                text.Span(statistics.TotalClassDays.ToString());
                            });
                        });

                        col.Item().Row(row =>
                        {
                            row.Spacing(15);

                            ComposeMetricBox(row.RelativeItem(), localizer["Pdf_Metric_AvgRealAttendance"],
                                $"{statistics.AverageRealAttendancePercentage}%", PrimaryColor);
                            ComposeMetricBox(row.RelativeItem(), localizer["Pdf_Metric_Below80"],
                                statistics.AtRiskCount.ToString(), OrangeColor);
                            ComposeMetricBox(row.RelativeItem(), localizer["Pdf_Metric_Below75"],
                                statistics.BelowDropThresholdCount.ToString(), RedColor);
                            ComposeMetricBox(row.RelativeItem(), localizer["Pdf_Metric_TotalStudents"],
                                statistics.Students.Count.ToString(), PrimaryColor);
                        });

                        col.Item().Element(e => ComposeStatisticsTable(e, statistics.Students));
                    });
                });

                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateAttendanceSessionPdf(AttendanceSessionDto session)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(30);
                page.MarginVertical(25);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(header => ComposeHeader(header, localizer["Pdf_Title_Record"], session.CourseName));

                page.Content().Element(content =>
                {
                    content.PaddingVertical(8).Column(col =>
                    {
                        col.Spacing(8);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text(text =>
                            {
                                text.Span(localizer["Pdf_Label_Course"] + " ").Bold();
                                text.Span(session.CourseName);
                            });
                            row.RelativeItem().Text(text =>
                            {
                                text.Span(localizer["Pdf_Label_Date"] + " ").Bold();
                                text.Span(session.Date.ToDateTime(TimeOnly.MinValue).ToString("d", CultureInfo.CurrentCulture));
                            });
                            row.RelativeItem().Text(text =>
                            {
                                text.Span(localizer["Pdf_Label_Students"] + " ").Bold();
                                text.Span(session.Entries.Count.ToString());
                            });
                        });

                        col.Item().Element(e => ComposeAttendanceTable(e, session.Entries));
                    });
                });

                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, string title, string courseName)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(innerCol =>
                {
                    innerCol.Item().Text("Tajamar").Bold().FontSize(12).FontColor(PrimaryColor);
                    innerCol.Item().Text(title).FontSize(16).Bold().FontColor(PrimaryColor);
                });
                row.ConstantItem(150).AlignRight().AlignMiddle()
                    .Text(DateTime.Now.ToString("g", CultureInfo.CurrentCulture)).FontSize(8).FontColor("#666666");
            });
            col.Item().PaddingTop(4).LineHorizontal(1).LineColor(PrimaryColor);
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().LineHorizontal(0.5f).LineColor("#cccccc");
            col.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span(localizer["Pdf_Footer_GeneratedOn"] + " ").FontSize(7).FontColor("#999999");
                    text.Span(DateTime.Now.ToString("G", CultureInfo.CurrentCulture)).FontSize(7).FontColor("#999999");
                });
                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span(localizer["Pdf_Footer_Page"] + " ").FontSize(7).FontColor("#999999");
                    text.CurrentPageNumber().FontSize(7).FontColor("#999999");
                    text.Span($" {localizer["Pdf_Footer_Of"]} ").FontSize(7).FontColor("#999999");
                    text.TotalPages().FontSize(7).FontColor("#999999");
                });
            });
        });
    }

    private static void ComposeMetricBox(IContainer container, string label, string value, string color)
    {
        container
            .Border(0.5f)
            .BorderColor("#e0e0e0")
            .Background("#fafafa")
            .Padding(8)
            .Column(col =>
            {
                col.Item().AlignCenter().Text(value).Bold().FontSize(14).FontColor(color);
                col.Item().AlignCenter().Text(label).FontSize(7).FontColor("#666666");
            });
    }

    private void ComposeStatisticsTable(IContainer container, IReadOnlyList<StudentStatisticsDto> students)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(25);   // #
                cols.RelativeColumn(3);    // Student
                cols.ConstantColumn(28);   // F
                cols.ConstantColumn(28);   // R
                cols.ConstantColumn(28);   // SAF
                cols.ConstantColumn(42);   // Weighted
                cols.ConstantColumn(42);   // Real %
                cols.ConstantColumn(38);   // F%
                cols.ConstantColumn(42);   // F+R%
                cols.ConstantColumn(58);   // Diploma
            });

            table.Header(header =>
            {
                var headerStyle = TextStyle.Default.Bold().FontSize(7).FontColor("#ffffff");

                header.Cell().Background(PrimaryColor).Padding(4).Text("#").Style(headerStyle);
                header.Cell().Background(PrimaryColor).Padding(4).Text(localizer["Pdf_Table_Student"]).Style(headerStyle);
                header.Cell().Background(PrimaryColor).Padding(4).AlignCenter().Text("F").Style(headerStyle);
                header.Cell().Background(PrimaryColor).Padding(4).AlignCenter().Text("R").Style(headerStyle);
                header.Cell().Background(PrimaryColor).Padding(4).AlignCenter().Text("SAF").Style(headerStyle);
                header.Cell().Background(PrimaryColor).Padding(4).AlignCenter().Text(localizer["Pdf_Table_Weighted"]).Style(headerStyle);
                header.Cell().Background(PrimaryColor).Padding(4).AlignCenter().Text(localizer["Pdf_Table_RealPct"]).Style(headerStyle);
                header.Cell().Background(PrimaryColor).Padding(4).AlignCenter().Text("F%").Style(headerStyle);
                header.Cell().Background(PrimaryColor).Padding(4).AlignCenter().Text("F+R%").Style(headerStyle);
                header.Cell().Background(PrimaryColor).Padding(4).AlignCenter().Text(localizer["Pdf_Table_Diploma"]).Style(headerStyle);
            });

            var cellStyle = TextStyle.Default.FontSize(7);

            for (var i = 0; i < students.Count; i++)
            {
                var student = students[i];
                var bg = i % 2 == 1 ? AltRowBg : "#ffffff";

                table.Cell().Background(bg).Padding(3).Text(student.Rank.ToString()).Style(cellStyle);
                table.Cell().Background(bg).Padding(3).Text(student.FullName).Style(cellStyle);
                table.Cell().Background(bg).Padding(3).AlignCenter().Text(student.AbsentCount.ToString()).Style(cellStyle);
                table.Cell().Background(bg).Padding(3).AlignCenter().Text(student.LateCount.ToString()).Style(cellStyle);
                table.Cell().Background(bg).Padding(3).AlignCenter().Text(student.EarlyLeaveCount.ToString()).Style(cellStyle);
                table.Cell().Background(bg).Padding(3).AlignCenter().Text(student.UnjustifiedWeighted.ToString("0.#")).Style(cellStyle);
                table.Cell().Background(bg).Padding(3).AlignCenter().Text($"{student.RealAttendancePercentage}%").Style(cellStyle);
                table.Cell().Background(bg).Padding(3).AlignCenter().Text($"{student.AbsentFPercentage}%").Style(cellStyle);
                table.Cell().Background(bg).Padding(3).AlignCenter().Text($"{student.AbsentFRPercentage}%").Style(cellStyle);

                var (diplomaText, diplomaColor) = student switch
                {
                    { DiplomaEligible: true } => (localizer["Pdf_Diploma_Ok"].Value, GreenColor),
                    { AtRiskDrop: true } => (localizer["Pdf_Diploma_AtRisk"].Value, RedColor),
                    _ => (localizer["Pdf_Diploma_Below80"].Value, OrangeColor)
                };

                table.Cell().Background(bg).Padding(3).AlignCenter()
                    .Text(diplomaText).Bold().FontSize(7).FontColor(diplomaColor);
            }
        });
    }

    private void ComposeAttendanceTable(IContainer container, IReadOnlyList<AttendanceEntryDto> entries)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(30);   // #
                cols.RelativeColumn(3);    // Student
                cols.RelativeColumn(2);    // Status
                cols.RelativeColumn(3);    // Comment
            });

            table.Header(header =>
            {
                var headerStyle = TextStyle.Default.Bold().FontSize(8).FontColor("#ffffff");

                header.Cell().Background(PrimaryColor).Padding(5).Text("#").Style(headerStyle);
                header.Cell().Background(PrimaryColor).Padding(5).Text(localizer["Pdf_Table_Student"]).Style(headerStyle);
                header.Cell().Background(PrimaryColor).Padding(5).Text(localizer["Pdf_Table_Status"]).Style(headerStyle);
                header.Cell().Background(PrimaryColor).Padding(5).Text(localizer["Pdf_Table_Comment"]).Style(headerStyle);
            });

            var cellStyle = TextStyle.Default.FontSize(8);

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var bg = i % 2 == 1 ? AltRowBg : "#ffffff";
                var statusLabel = GetStatusLabel(entry.Status);

                table.Cell().Background(bg).Padding(4).Text((i + 1).ToString()).Style(cellStyle);
                table.Cell().Background(bg).Padding(4).Text(entry.StudentFullName).Style(cellStyle);
                table.Cell().Background(bg).Padding(4).Text(statusLabel).Style(cellStyle)
                    .FontColor(GetStatusColor(entry.Status));
                table.Cell().Background(bg).Padding(4).Text(entry.Comment ?? localizer["Pdf_Placeholder_None"]).Style(cellStyle).FontColor("#666666");
            }
        });
    }

    private string GetStatusLabel(AttendanceStatus status) => status switch
    {
        AttendanceStatus.Present => localizer["Status_Present"],
        AttendanceStatus.Absent => localizer["Status_AbsentF"],
        AttendanceStatus.Late => localizer["Status_LateR"],
        AttendanceStatus.JustifiedAbsent => localizer["Status_JustifiedAbsentFJ"],
        AttendanceStatus.JustifiedLate => localizer["Status_JustifiedLateRJ"],
        AttendanceStatus.EarlyLeave => localizer["Status_EarlyLeaveSAF"],
        AttendanceStatus.JustifiedEarlyLeave => localizer["Status_JustifiedEarlyLeaveSAFJ"],
        _ => status.ToString()
    };

    private static string GetStatusColor(AttendanceStatus status) => status switch
    {
        AttendanceStatus.Present => GreenColor,
        AttendanceStatus.Absent => RedColor,
        AttendanceStatus.Late => OrangeColor,
        AttendanceStatus.JustifiedAbsent => "#666666",
        AttendanceStatus.JustifiedLate => "#666666",
        AttendanceStatus.EarlyLeave => OrangeColor,
        AttendanceStatus.JustifiedEarlyLeave => "#666666",
        _ => "#000000"
    };
}
