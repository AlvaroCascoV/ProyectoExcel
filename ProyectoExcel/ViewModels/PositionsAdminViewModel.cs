using Attendance.Infrastructure.DTOs;

namespace MvcProyectoExcel.ViewModels;

public class PositionsAdminViewModel
{
    public IReadOnlyList<PositionAdminDto> Positions { get; set; } = [];

    // All students (for resolving assigned student name in the table)
    public IReadOnlyList<StudentDto> AllStudents { get; set; } = [];

    // Students filtered by the selected course (for the dropdown)
    public IReadOnlyList<StudentDto> Students { get; set; } = [];

    public IReadOnlyList<CourseDto> Courses { get; set; } = [];

    public int? SelectedCourseId { get; set; }
}

