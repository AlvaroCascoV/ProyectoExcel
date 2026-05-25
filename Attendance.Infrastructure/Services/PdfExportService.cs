using Attendance.Infrastructure.DTOs;
using Attendance.Infrastructure.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Attendance.Infrastructure.Services;

public interface IPdfExportService
{
    byte[] GenerateCourseStatisticsPdf(CourseStatisticsDto statistics, int? month = null, int? year = null);
    byte[] GenerateAttendanceSessionPdf(AttendanceSessionDto session);
}

public class PdfExportService : IPdfExportService
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

                page.Header().Element(header => ComposeHeader(header, "Attendance Statistics Report", statistics.CourseName));

                page.Content().Element(content =>
                {
                    content.PaddingVertical(8).Column(col =>
                    {
                        col.Spacing(8);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text(text =>
                            {
                                text.Span("Course: ").Bold();
                                text.Span(statistics.CourseName);
                            });

                            if (month.HasValue || year.HasValue)
                            {
                                row.RelativeItem().Text(text =>
                                {
                                    text.Span("Period: ").Bold();
                                    var parts = new List<string>();
                                    if (month.HasValue) parts.Add($"Month {month.Value}");
                                    if (year.HasValue) parts.Add($"Year {year.Value}");
                                    text.Span(string.Join(", ", parts));
                                });
                            }

                            row.RelativeItem().Text(text =>
                            {
                                text.Span("Lective days: ").Bold();
                                text.Span(statistics.TotalClassDays.ToString());
                            });
                        });

                        col.Item().Row(row =>
                        {
                            row.Spacing(15);

                            ComposeMetricBox(row.RelativeItem(), "Avg. Real Attendance",
                                $"{statistics.AverageRealAttendancePercentage}%", PrimaryColor);
                            ComposeMetricBox(row.RelativeItem(), "Below 80% (diploma)",
                                statistics.AtRiskCount.ToString(), OrangeColor);
                            ComposeMetricBox(row.RelativeItem(), "Below 75% (at risk)",
                                statistics.BelowDropThresholdCount.ToString(), RedColor);
                            ComposeMetricBox(row.RelativeItem(), "Total Students",
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

                page.Header().Element(header => ComposeHeader(header, "Attendance Record", session.CourseName));

                page.Content().Element(content =>
                {
                    content.PaddingVertical(8).Column(col =>
                    {
                        col.Spacing(8);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text(text =>
                            {
                                text.Span("Course: ").Bold();
                                text.Span(session.CourseName);
                            });
                            row.RelativeItem().Text(text =>
                            {
                                text.Span("Date: ").Bold();
                                text.Span(session.Date.ToString("yyyy-MM-dd"));
                            });
                            row.RelativeItem().Text(text =>
                            {
                                text.Span("Students: ").Bold();
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

    private static void ComposeHeader(IContainer container, string title, string courseName)
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
                    .Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(8).FontColor("#666666");
            });
            col.Item().PaddingTop(4).LineHorizontal(1).LineColor(PrimaryColor);
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().LineHorizontal(0.5f).LineColor("#cccccc");
            col.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span("Generated on ").FontSize(7).FontColor("#999999");
                    text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")).FontSize(7).FontColor("#999999");
                });
                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Page ").FontSize(7).FontColor("#999999");
                    text.CurrentPageNumber().FontSize(7).FontColor("#999999");
                    text.Span(" of ").FontSize(7).FontColor("#999999");
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

    private static void ComposeStatisticsTable(IContainer container, IReadOnlyList<StudentStatisticsDto> students)
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
                header.Cell().Background(PrimaryColor).Padding(4).Text("Student").Style(headerStyle);
                header.Cell().Background(PrimaryColor).Padding(4).AlignCenter().Text("F").Style(headerStyle);
                header.Cell().Background(PrimaryColor).Padding(4).AlignCenter().Text("R").Style(headerStyle);
                header.Cell().Background(PrimaryColor).Padding(4).AlignCenter().Text("SAF").Style(headerStyle);
                header.Cell().Background(PrimaryColor).Padding(4).AlignCenter().Text("Weighted").Style(headerStyle);
                header.Cell().Background(PrimaryColor).Padding(4).AlignCenter().Text("Real %").Style(headerStyle);
                header.Cell().Background(PrimaryColor).Padding(4).AlignCenter().Text("F%").Style(headerStyle);
                header.Cell().Background(PrimaryColor).Padding(4).AlignCenter().Text("F+R%").Style(headerStyle);
                header.Cell().Background(PrimaryColor).Padding(4).AlignCenter().Text("Diploma").Style(headerStyle);
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
                    { DiplomaEligible: true } => ("OK", GreenColor),
                    { AtRiskDrop: true } => ("At risk", RedColor),
                    _ => ("Below 80%", OrangeColor)
                };

                table.Cell().Background(bg).Padding(3).AlignCenter()
                    .Text(diplomaText).Bold().FontSize(7).FontColor(diplomaColor);
            }
        });
    }

    private static void ComposeAttendanceTable(IContainer container, IReadOnlyList<AttendanceEntryDto> entries)
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
                header.Cell().Background(PrimaryColor).Padding(5).Text("Student").Style(headerStyle);
                header.Cell().Background(PrimaryColor).Padding(5).Text("Status").Style(headerStyle);
                header.Cell().Background(PrimaryColor).Padding(5).Text("Comment").Style(headerStyle);
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
                table.Cell().Background(bg).Padding(4).Text(entry.Comment ?? "—").Style(cellStyle).FontColor("#666666");
            }
        });
    }

    private static string GetStatusLabel(AttendanceStatus status) => status switch
    {
        AttendanceStatus.Present => "Present",
        AttendanceStatus.Absent => "Absent (F)",
        AttendanceStatus.Late => "Late (R)",
        AttendanceStatus.JustifiedAbsent => "Justified absent (FJ)",
        AttendanceStatus.JustifiedLate => "Justified late (RJ)",
        AttendanceStatus.EarlyLeave => "Early leave (SAF)",
        AttendanceStatus.JustifiedEarlyLeave => "Justified early leave (SAFJ)",
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
