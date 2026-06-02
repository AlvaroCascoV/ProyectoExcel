using Attendance.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using MvcProyectoExcel.Services;
using MvcProyectoExcel.ViewModels;

namespace MvcProyectoExcel.Controllers;

[Authorize(Roles = $"{AppRoles.Teacher},{AppRoles.Admin}")]
public class PositionsController(IAttendanceApiClient apiClient, IStringLocalizer<SharedResource> localizer) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(int? courseId, CancellationToken cancellationToken)
    {
        try
        {
            var positions = await apiClient.GetPositionsAdminAsync(cancellationToken);
            var courses = await apiClient.GetCoursesAsync(activeOnly: true, cancellationToken);
            var selectedCourseId = courseId ?? courses.FirstOrDefault()?.Id;

            var allStudents = await apiClient.GetAllStudentsAdminAsync(cancellationToken);
            var filteredStudents = selectedCourseId.HasValue
                ? await apiClient.GetStudentsByCourseAsync(selectedCourseId.Value, cancellationToken)
                : allStudents;

            return View(new PositionsAdminViewModel
            {
                Positions = positions,
                Courses = courses,
                SelectedCourseId = selectedCourseId,
                AllStudents = allStudents,
                Students = filteredStudents
            });
        }
        catch (HttpRequestException)
        {
            ViewBag.ErrorMessage = localizer["ErrorApiUnreachable"].Value;
            return View(new PositionsAdminViewModel
            {
                Positions = [],
                Courses = [],
                AllStudents = [],
                Students = [],
                SelectedCourseId = courseId
            });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignUser(int positionId, int tajamarUserId, CancellationToken cancellationToken)
    {
        try
        {
            await apiClient.AssignPositionToUserAsync(positionId, tajamarUserId, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] = localizer["ErrorApiUnreachable"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Seed(string classCode, int from, int to, CancellationToken cancellationToken)
    {
        try
        {
            await apiClient.SeedPositionsAsync(classCode, from, to, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] = localizer["ErrorApiUnreachable"].Value;
            return RedirectToAction(nameof(Index));
        }
    }
}

