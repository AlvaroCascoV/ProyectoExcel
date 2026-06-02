using System.Security.Claims;
using Attendance.Infrastructure.DTOs;
using Attendance.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace ApiProyectoExcel.Controllers;

[ApiController]
[Route("api")]
public class AttendanceController(
    IAttendanceService attendanceService,
    ICourseService courseService,
    IPdfExportService pdfExportService,
    IWebHostEnvironment environment) : ControllerBase
{
    [Authorize(Roles = $"{AppRoles.Teacher},{AppRoles.Admin}")]
    [HttpGet("courses/{courseId:int}/attendance")]
    public async Task<IActionResult> GetSession(
        int courseId,
        [FromQuery] DateOnly date,
        CancellationToken cancellationToken = default)
    {
        if (!CanAccessCourse(courseId))
        {
            return Forbid();
        }

        var session = await attendanceService.GetSessionSheetAsync(courseId, date, cancellationToken);
        if (session is null)
        {
            return NotFound(new { message = $"Course {courseId} not found." });
        }

        return Ok(session);
    }

    [Authorize(Roles = $"{AppRoles.Teacher},{AppRoles.Admin}")]
    [HttpPut("courses/{courseId:int}/attendance")]
    public async Task<IActionResult> SaveSession(
        int courseId,
        [FromQuery] DateOnly date,
        [FromBody] SaveAttendanceSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!CanAccessCourse(courseId))
        {
            return Forbid();
        }

        if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            return BadRequest(new { message = "Cannot save attendance on weekends." });

        if (request.Entries.Count == 0)
        {
            return BadRequest(new { message = "At least one attendance entry is required." });
        }

        if (!TryGetTajamarUserId(out var teacherId))
        {
            return Unauthorized(new { message = "Could not resolve the current user." });
        }

        var (success, error) = await attendanceService.SaveSessionAsync(
            courseId,
            date,
            request.Entries,
            teacherId,
            cancellationToken);

        if (!success)
        {
            return error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { message = error })
                : BadRequest(new { message = error });
        }

        var session = await attendanceService.GetSessionSheetAsync(courseId, date, cancellationToken);
        return Ok(session);
    }

    [Authorize(Roles = $"{AppRoles.Teacher},{AppRoles.Admin}")]
    [HttpGet("courses/{courseId:int}/attendance/dates")]
    public async Task<IActionResult> GetSessionDates(int courseId, CancellationToken cancellationToken = default)
    {
        if (!CanAccessCourse(courseId))
        {
            return Forbid();
        }

        var dates = await attendanceService.GetSessionDatesAsync(courseId, cancellationToken);
        return Ok(dates);
    }

    [Authorize(Roles = $"{AppRoles.Teacher},{AppRoles.Admin}")]
    [HttpPost("courses/{courseId:int}/attendance/seed-present")]
    public async Task<IActionResult> SeedPresentDays(
        int courseId,
        [FromQuery] int days = 7,
        CancellationToken cancellationToken = default)
    {
        if (!CanAccessCourse(courseId))
        {
            return Forbid();
        }

        if (!environment.IsDevelopment())
        {
            return NotFound(new { message = "This endpoint is only available in Development." });
        }

        if (!TryGetTajamarUserId(out var teacherId))
        {
            return Unauthorized(new { message = "Could not resolve the current user." });
        }

        var (success, error, result) = await attendanceService.SeedPresentDaysAsync(
            courseId,
            teacherId,
            days,
            cancellationToken: cancellationToken);

        if (!success || result is null)
        {
            return error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { message = error })
                : BadRequest(new { message = error });
        }

        return Ok(result);
    }

    [Authorize(Roles = AppRoles.Student)]
    [HttpGet("attendance/me/courses")]
    public async Task<IActionResult> GetMyCourses(CancellationToken cancellationToken = default)
    {
        if (!TryGetTajamarUserId(out var studentId))
        {
            return Unauthorized(new { message = "Could not resolve the current user." });
        }

        var courses = await courseService.GetCoursesForUserAsync(studentId, cancellationToken);
        return Ok(courses);
    }

    [Authorize(Roles = AppRoles.Student)]
    [HttpGet("attendance/me")]
    public async Task<IActionResult> GetMyAttendance(
        [FromQuery] int courseId = 0,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetTajamarUserId(out var studentId))
        {
            return Unauthorized(new { message = "Could not resolve the current user." });
        }

        var records = await attendanceService.GetStudentAttendanceAsync(
            studentId,
            courseId > 0 ? courseId : null,
            from,
            to,
            cancellationToken);

        return Ok(records);
    }

    [Authorize(Roles = AppRoles.Student)]
    [HttpGet("attendance/me/summary")]
    public async Task<IActionResult> GetMySummary(
        [FromQuery] int courseId = 0,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetTajamarUserId(out var studentId))
        {
            return Unauthorized(new { message = "Could not resolve the current user." });
        }

        if (courseId <= 0)
        {
            var summaries = await attendanceService.GetStudentSummariesAsync(studentId, cancellationToken);
            return Ok(summaries);
        }

        var summary = await attendanceService.GetStudentSummaryAsync(studentId, courseId, cancellationToken);
        if (summary is null)
        {
            return NotFound(new { message = $"Course {courseId} not found or you are not enrolled." });
        }

        return Ok(summary);
    }

    [Authorize(Roles = $"{AppRoles.Teacher},{AppRoles.Admin}")]
    [HttpGet("courses/{courseId:int}/attendance/export/pdf")]
    public async Task<IActionResult> ExportAttendanceSessionPdf(
        int courseId,
        [FromQuery] DateOnly date,
        CancellationToken cancellationToken = default)
    {
        if (!CanAccessCourse(courseId))
        {
            return Forbid();
        }

        var session = await attendanceService.GetSessionSheetAsync(courseId, date, cancellationToken);
        if (session is null)
        {
            return NotFound(new { message = $"Course {courseId} not found." });
        }

        var pdfBytes = pdfExportService.GenerateAttendanceSessionPdf(session);
        var safeName = session.CourseName.Replace(" ", "-", StringComparison.Ordinal);
        var fileName = $"attendance-{safeName}-{date:yyyy-MM-dd}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    private bool TryGetTajamarUserId(out int userId)
    {
        var claim = User.FindFirstValue("tajamar_user_id");
        return int.TryParse(claim, out userId);
    }

    private bool CanAccessCourse(int courseId)
    {
        if (User.IsInRole(AppRoles.Admin))
        {
            return true;
        }

        return User.FindAll("course_id")
            .Select(c => c.Value)
            .Any(v => int.TryParse(v, out var id) && id == courseId);
    }
}
