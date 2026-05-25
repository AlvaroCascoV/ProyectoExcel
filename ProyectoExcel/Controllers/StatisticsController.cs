using Attendance.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MvcProyectoExcel.Services;
using MvcProyectoExcel.ViewModels;

namespace MvcProyectoExcel.Controllers;

[Authorize(Roles = $"{AppRoles.Teacher},{AppRoles.Admin}")]
public class StatisticsController(IAttendanceApiClient apiClient, IWebHostEnvironment environment) : Controller
{
    private const int DefaultCourseId = 3430;

    [HttpGet]
    public async Task<IActionResult> Index(
        int? courseId,
        int? month,
        int? year,
        decimal? minPercent,
        decimal? maxPercent,
        CancellationToken cancellationToken)
    {
        try
        {
            var courses = await apiClient.GetCoursesAsync(cancellationToken: cancellationToken);
            var selectedId = courseId ?? courses.FirstOrDefault(c => c.Id == DefaultCourseId)?.Id ?? courses.FirstOrDefault()?.Id;

            var model = new StatisticsDashboardViewModel
            {
                Courses = courses,
                SelectedCourseId = selectedId,
                SelectedMonth = month,
                SelectedYear = year,
                MinPercent = minPercent,
                MaxPercent = maxPercent
            };

            if (!selectedId.HasValue)
            {
                return View(model);
            }

            model.SelectedCourseName = courses.FirstOrDefault(c => c.Id == selectedId)?.Name ?? string.Empty;
            model.Statistics = await apiClient.GetCourseStatisticsAsync(
                selectedId.Value,
                month,
                year,
                minPercent,
                maxPercent,
                cancellationToken);

            if (model.Statistics is not null)
            {
                model.AtRiskRankings = await apiClient.GetCourseRankingsAsync(
                    selectedId.Value,
                    ascending: true,
                    top: 5,
                    month,
                    year,
                    cancellationToken);

                model.ChartData = StatisticsChartDataBuilder.From(model.Statistics, model.AtRiskRankings);
            }

            return View(model);
        }
        catch (HttpRequestException)
        {
            return View(new StatisticsDashboardViewModel
            {
                ErrorMessage = "Could not reach the API. Make sure ApiProyectoExcel is running on http://localhost:5180."
            });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SeedPresentWeek(int courseId, CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        try
        {
            var result = await apiClient.SeedPresentDaysAsync(courseId, days: 7, cancellationToken);
            if (result is null)
            {
                TempData["ErrorMessage"] = "Seed endpoint is not available (Development only).";
            }
            else
            {
                TempData["SuccessMessage"] =
                    $"Marked {result.StudentsAffected} students Present for {result.DaysSeeded} days ({result.RecordsUpserted} records).";
            }
        }
        catch (HttpRequestException ex)
        {
            TempData["ErrorMessage"] = $"Could not seed attendance: {ex.Message}";
        }

        return RedirectToAction(nameof(Index), new { courseId });
    }

    [HttpGet]
    public async Task<IActionResult> ExportPdf(
        int courseId,
        int? month,
        int? year,
        decimal? minPercent,
        decimal? maxPercent,
        CancellationToken cancellationToken)
    {
        try
        {
            var pdfBytes = await apiClient.ExportCourseStatisticsPdfAsync(
                courseId, month, year, minPercent, maxPercent, cancellationToken);

            if (pdfBytes is null)
            {
                TempData["ErrorMessage"] = "Could not generate PDF: course not found.";
                return RedirectToAction(nameof(Index), new { courseId, month, year, minPercent, maxPercent });
            }

            var fileName = $"statistics-{courseId}-{DateTime.Now:yyyy-MM-dd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] = "Could not generate PDF. Make sure ApiProyectoExcel is running.";
            return RedirectToAction(nameof(Index), new { courseId, month, year, minPercent, maxPercent });
        }
    }
}
