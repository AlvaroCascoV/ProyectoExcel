using Attendance.Infrastructure.DTOs;

namespace MvcProyectoExcel.ViewModels;

public class StudentDashboardViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IReadOnlyList<CourseDto> Courses { get; set; } = [];
    public int? SelectedCourseId { get; set; }
    public string SelectedCourseName { get; set; } = string.Empty;
    public AttendanceSummaryDto? Summary { get; set; }
    public IReadOnlyList<AttendanceRecordDto> Records { get; set; } = [];
    public string? ErrorMessage { get; set; }
}
