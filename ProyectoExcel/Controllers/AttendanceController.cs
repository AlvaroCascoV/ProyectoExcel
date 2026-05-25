using Attendance.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using MvcProyectoExcel.Services;
using MvcProyectoExcel.ViewModels;

namespace MvcProyectoExcel.Controllers;

[Authorize(Roles = $"{AppRoles.Teacher},{AppRoles.Admin}")]
public class AttendanceController(IAttendanceApiClient apiClient, IStringLocalizer<SharedResource> localizer) : Controller
{
    private const int DefaultCourseId = 3430;

    [HttpGet]
    public async Task<IActionResult> Index(int? courseId, DateOnly? date, CancellationToken cancellationToken)
    {
        try
        {
            var courses = await apiClient.GetCoursesAsync(cancellationToken: cancellationToken);
            var selectedId = courseId ?? courses.FirstOrDefault(c => c.Id == DefaultCourseId)?.Id ?? courses.FirstOrDefault()?.Id;
            var selectedDate = date ?? DateOnly.FromDateTime(DateTime.Today);

            var model = new AttendancePassListViewModel
            {
                Courses = courses,
                SelectedCourseId = selectedId,
                SelectedDate = selectedDate
            };

            if (selectedId.HasValue)
            {
                model.SelectedCourseName = courses.FirstOrDefault(c => c.Id == selectedId)?.Name ?? string.Empty;
                model.PreviousDates = await apiClient.GetAttendanceDatesAsync(selectedId.Value, cancellationToken);

                var session = await apiClient.GetAttendanceSessionAsync(selectedId.Value, selectedDate, cancellationToken);
                if (session is not null)
                {
                    model.Rows = session.Entries
                        .Select(e => new AttendanceRowViewModel
                        {
                            StudentId = e.StudentId,
                            StudentFullName = e.StudentFullName,
                            Status = e.Status,
                            Comment = e.Comment
                        })
                        .ToList();
                }
            }

            return View(model);
        }
        catch (HttpRequestException)
        {
            return View(new AttendancePassListViewModel
            {
                ErrorMessage = localizer["ErrorApiUnreachable"]
            });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(AttendancePassListViewModel model, CancellationToken cancellationToken)
    {
        if (!model.SelectedCourseId.HasValue)
        {
            ModelState.AddModelError(string.Empty, localizer["ErrorSelectCourse"]);
            return await Index(null, model.SelectedDate, cancellationToken);
        }

        try
        {
            if (model.Rows.Count == 0)
            {
                model.ErrorMessage = localizer["ErrorNoRowsSubmitted"];
                return await ReloadIndexView(model, cancellationToken);
            }

            var entries = model.Rows
                .Select(r => new Attendance.Infrastructure.DTOs.SaveAttendanceEntryRequest(
                    r.StudentId,
                    r.Status,
                    r.Comment))
                .ToList();

            await apiClient.SaveAttendanceSessionAsync(
                model.SelectedCourseId.Value,
                model.SelectedDate,
                new Attendance.Infrastructure.DTOs.SaveAttendanceSessionRequest(entries),
                cancellationToken);

            TempData["SuccessMessage"] = string.Format(localizer["SuccessAttendanceSaved"], model.SelectedDate.ToString("yyyy-MM-dd"));
            return RedirectToAction(nameof(Index), new { courseId = model.SelectedCourseId, date = model.SelectedDate.ToString("yyyy-MM-dd") });
        }
        catch (HttpRequestException ex)
        {
            model.ErrorMessage = string.Format(localizer["ErrorCouldNotSave"], ex.Message);
            return await ReloadIndexView(model, cancellationToken);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportPdf(int courseId, string date, CancellationToken cancellationToken)
    {
        try
        {
            if (!DateOnly.TryParse(date, out var parsedDate))
            {
                TempData["ErrorMessage"] = localizer["ErrorInvalidDate"];
                return RedirectToAction(nameof(Index), new { courseId });
            }

            var pdfBytes = await apiClient.ExportAttendanceSessionPdfAsync(courseId, parsedDate, cancellationToken);
            if (pdfBytes is null)
            {
                TempData["ErrorMessage"] = localizer["ErrorPdfSessionNotFound"];
                return RedirectToAction(nameof(Index), new { courseId, date });
            }

            var fileName = $"attendance-{courseId}-{parsedDate:yyyy-MM-dd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] = localizer["ErrorPdfGeneration"];
            return RedirectToAction(nameof(Index), new { courseId, date });
        }
    }

    private async Task<IActionResult> ReloadIndexView(AttendancePassListViewModel model, CancellationToken cancellationToken)
    {
        model.Courses = await apiClient.GetCoursesAsync(cancellationToken: cancellationToken);
        model.SelectedCourseName = model.Courses.FirstOrDefault(c => c.Id == model.SelectedCourseId)?.Name ?? string.Empty;

        if (model.SelectedCourseId.HasValue)
        {
            model.PreviousDates = await apiClient.GetAttendanceDatesAsync(model.SelectedCourseId.Value, cancellationToken);

            if (model.Rows.Count == 0)
            {
                var session = await apiClient.GetAttendanceSessionAsync(
                    model.SelectedCourseId.Value,
                    model.SelectedDate,
                    cancellationToken);

                if (session is not null)
                {
                    model.Rows = session.Entries
                        .Select(e => new AttendanceRowViewModel
                        {
                            StudentId = e.StudentId,
                            StudentFullName = e.StudentFullName,
                            Status = e.Status,
                            Comment = e.Comment
                        })
                        .ToList();
                }
            }
        }

        return View("Index", model);
    }
}
