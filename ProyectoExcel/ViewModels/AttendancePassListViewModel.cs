using Attendance.Infrastructure.DTOs;
using Attendance.Infrastructure.Entities;

namespace MvcProyectoExcel.ViewModels;

public class AttendancePassListViewModel
{
    public IReadOnlyList<CourseDto> Courses { get; set; } = [];
    public int? SelectedCourseId { get; set; }
    public string SelectedCourseName { get; set; } = string.Empty;
    public DateOnly SelectedDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public List<AttendanceRowViewModel> Rows { get; set; } = [];
    public IReadOnlyList<DateOnly> PreviousDates { get; set; } = [];
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public bool HasCalendar { get; set; }
    public int LectiveDaysCount { get; set; }
    public string? CalendarJson { get; set; }
}

public class AttendanceRowViewModel
{
    public int StudentId { get; set; }
    public string StudentFullName { get; set; } = string.Empty;
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public string? Comment { get; set; }
}

public static class AttendanceStatusDisplay
{
    public static string GetLabel(AttendanceStatus status) => status switch
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

    public static string GetBadgeClass(AttendanceStatus status) => status switch
    {
        AttendanceStatus.Present => "text-bg-success",
        AttendanceStatus.Absent => "text-bg-danger",
        AttendanceStatus.Late => "text-bg-warning",
        AttendanceStatus.JustifiedAbsent => "text-bg-secondary",
        AttendanceStatus.JustifiedLate => "text-bg-info",
        AttendanceStatus.EarlyLeave => "text-bg-warning",
        AttendanceStatus.JustifiedEarlyLeave => "text-bg-info",
        _ => "text-bg-light"
    };
}
