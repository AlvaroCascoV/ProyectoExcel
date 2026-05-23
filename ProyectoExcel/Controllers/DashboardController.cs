using Attendance.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcProyectoExcel.Services;
using MvcProyectoExcel.ViewModels;
using System.Security.Claims;

namespace MvcProyectoExcel.Controllers;

[Authorize(Roles = AppRoles.Student)]
public class DashboardController(IAttendanceApiClient apiClient) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(int? courseId, CancellationToken cancellationToken)
    {
        var model = new StudentDashboardViewModel
        {
            FullName = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
            Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty
        };

        try
        {
            model.Courses = await apiClient.GetMyCoursesAsync(cancellationToken);

            var selectedId = courseId ?? model.Courses.FirstOrDefault()?.Id;
            model.SelectedCourseId = selectedId;

            if (selectedId.HasValue)
            {
                model.SelectedCourseName = model.Courses.FirstOrDefault(c => c.Id == selectedId)?.Name ?? string.Empty;
                model.Summary = await apiClient.GetMySummaryAsync(selectedId.Value, cancellationToken);
                model.Records = await apiClient.GetMyAttendanceAsync(selectedId.Value, cancellationToken: cancellationToken);
            }

            return View(model);
        }
        catch (HttpRequestException)
        {
            model.ErrorMessage = "Could not reach the API. Make sure ApiProyectoExcel is running on http://localhost:5180.";
            return View(model);
        }
    }
}
