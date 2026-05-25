using Attendance.Infrastructure.DTOs;
using Attendance.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiProyectoExcel.Controllers;

[ApiController]
[Route("api/statistics")]
[Authorize(Roles = $"{AppRoles.Teacher},{AppRoles.Admin}")]
public class StatisticsController(IStatisticsService statisticsService, IPdfExportService pdfExportService) : ControllerBase
{
    [HttpGet("course/{courseId:int}")]
    public async Task<IActionResult> GetCourseStatistics(
        int courseId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int? month,
        [FromQuery] int? year,
        [FromQuery] decimal? minPercent,
        [FromQuery] decimal? maxPercent,
        CancellationToken cancellationToken = default)
    {
        var statistics = await statisticsService.GetCourseStatisticsAsync(
            courseId,
            from,
            to,
            month,
            year,
            minPercent,
            maxPercent,
            cancellationToken);

        if (statistics is null)
        {
            return NotFound(new { message = $"Course {courseId} not found." });
        }

        return Ok(statistics);
    }

    [HttpGet("course/{courseId:int}/rankings")]
    public async Task<IActionResult> GetCourseRankings(
        int courseId,
        [FromQuery] bool ascending = false,
        [FromQuery] int? top = null,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        [FromQuery] int? month = null,
        [FromQuery] int? year = null,
        CancellationToken cancellationToken = default)
    {
        var courseExists = await statisticsService.GetCourseStatisticsAsync(courseId, cancellationToken: cancellationToken);
        if (courseExists is null)
        {
            return NotFound(new { message = $"Course {courseId} not found." });
        }

        var rankings = await statisticsService.GetCourseRankingsAsync(
            courseId,
            ascending,
            top,
            from,
            to,
            month,
            year,
            cancellationToken);

        return Ok(rankings);
    }

    [HttpGet("course/{courseId:int}/export/pdf")]
    public async Task<IActionResult> ExportCourseStatisticsPdf(
        int courseId,
        [FromQuery] int? month,
        [FromQuery] int? year,
        [FromQuery] decimal? minPercent,
        [FromQuery] decimal? maxPercent,
        CancellationToken cancellationToken = default)
    {
        var statistics = await statisticsService.GetCourseStatisticsAsync(
            courseId, month: month, year: year,
            minPercent: minPercent, maxPercent: maxPercent,
            cancellationToken: cancellationToken);

        if (statistics is null)
        {
            return NotFound(new { message = $"Course {courseId} not found." });
        }

        var pdfBytes = pdfExportService.GenerateCourseStatisticsPdf(statistics, month, year);
        var safeName = statistics.CourseName.Replace(" ", "-", StringComparison.Ordinal);
        var fileName = $"statistics-{safeName}-{DateTime.Now:yyyy-MM-dd}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}
