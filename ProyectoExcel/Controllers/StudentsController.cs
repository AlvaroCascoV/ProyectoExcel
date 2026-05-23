using Attendance.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcProyectoExcel.Services;
using MvcProyectoExcel.ViewModels;

namespace MvcProyectoExcel.Controllers;

[Authorize(Roles = $"{AppRoles.Teacher},{AppRoles.Admin}")]
public class StudentsController(IAttendanceApiClient apiClient) : Controller
{
    private const int DefaultCourseId = 3430;

    [HttpGet]
    public async Task<IActionResult> Index(int? courseId, CancellationToken cancellationToken)
    {
        try
        {
            var courses = await apiClient.GetCoursesAsync(cancellationToken: cancellationToken);
            var selectedId = courseId ?? courses.FirstOrDefault(c => c.Id == DefaultCourseId)?.Id ?? courses.FirstOrDefault()?.Id;

            IReadOnlyList<Attendance.Infrastructure.DTOs.StudentDto> students = [];
            string? courseName = null;

            if (selectedId.HasValue)
            {
                students = await apiClient.GetStudentsByCourseAsync(selectedId.Value, cancellationToken);
                courseName = courses.FirstOrDefault(c => c.Id == selectedId)?.Name;
            }

            return View(new StudentListViewModel
            {
                Courses = courses,
                Students = students,
                SelectedCourseId = selectedId,
                SelectedCourseName = courseName
            });
        }
        catch (HttpRequestException)
        {
            return View(new StudentListViewModel
            {
                ErrorMessage = "Could not reach the API. Make sure ApiProyectoExcel is running on http://localhost:5180."
            });
        }
    }
}
