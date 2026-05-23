using Attendance.Infrastructure.DTOs;

namespace MvcProyectoExcel.ViewModels;

public class StudentListViewModel
{
    public IReadOnlyList<CourseDto> Courses { get; init; } = [];
    public IReadOnlyList<StudentDto> Students { get; init; } = [];
    public int? SelectedCourseId { get; init; }
    public string? SelectedCourseName { get; init; }
    public string? ErrorMessage { get; init; }
}
