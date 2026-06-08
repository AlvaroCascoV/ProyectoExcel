using System.ComponentModel.DataAnnotations;
using Attendance.Infrastructure.DTOs;

namespace MvcProyectoExcel.ViewModels;

public class StatisticsDashboardViewModel
{
    public IReadOnlyList<CourseDto> Courses { get; set; } = [];
    public int? SelectedCourseId { get; set; }
    public string SelectedCourseName { get; set; } = string.Empty;
    public int? SelectedMonth { get; set; }
    public int? SelectedYear { get; set; }
    [Range(0, 100, ErrorMessage = "Minimum must be between 0 and 100")]
    public decimal? MinPercent { get; set; }

    [Range(0, 100, ErrorMessage = "Maximum must be between 0 and 100")]
    public decimal? MaxPercent { get; set; }
    public CourseStatisticsDto? Statistics { get; set; }
    public IReadOnlyList<RankingEntryDto> AtRiskRankings { get; set; } = [];
    public StatisticsChartData ChartData { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class StatisticsChartData
{
    public string[] StatusLabels { get; set; } = [];
    public int[] StatusValues { get; set; } = [];
    public string[] BucketLabels { get; set; } = [];
    public int[] BucketValues { get; set; } = [];
    public string[] TopStudentLabels { get; set; } = [];
    public decimal[] TopStudentValues { get; set; } = [];
    public string[] AtRiskLabels { get; set; } = [];
    public decimal[] AtRiskValues { get; set; } = [];
}

public static class StatisticsChartDataBuilder
{
    public static StatisticsChartData From(CourseStatisticsDto statistics, IReadOnlyList<RankingEntryDto> atRiskRankings, string[]? statusLabels = null)
    {
        var students = statistics.Students;

        return new StatisticsChartData
        {
            StatusLabels = statusLabels ?? ["Present", "Absent", "Late", "FJ", "RJ", "SAF", "SAFJ"],
            StatusValues =
            [
                students.Sum(s => s.PresentCount),
                students.Sum(s => s.AbsentCount),
                students.Sum(s => s.LateCount),
                students.Sum(s => s.JustifiedAbsentCount),
                students.Sum(s => s.JustifiedLateCount),
                students.Sum(s => s.EarlyLeaveCount),
                students.Sum(s => s.JustifiedEarlyLeaveCount)
            ],
            BucketLabels = ["90-100%", "80-89%", "75-79%"],
            BucketValues =
            [
                students.Count(s => s.RealAttendancePercentage >= 90),
                students.Count(s => s.RealAttendancePercentage is >= 80 and < 90),
                students.Count(s => s.RealAttendancePercentage is >= 75 and < 80)
            ],
            TopStudentLabels = statistics.Students
                .OrderByDescending(s => s.RealAttendancePercentage)
                .Take(5)
                .Select(s => s.FullName)
                .ToArray(),
            TopStudentValues = statistics.Students
                .OrderByDescending(s => s.RealAttendancePercentage)
                .Take(5)
                .Select(s => s.RealAttendancePercentage)
                .ToArray(),
            AtRiskLabels = atRiskRankings
                .Select(r => r.FullName)
                .ToArray(),
            AtRiskValues = atRiskRankings
                .Select(r => r.RealAttendancePercentage)
                .ToArray()
        };
    }
}
