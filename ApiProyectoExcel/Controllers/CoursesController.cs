using Attendance.Infrastructure.DTOs;
using Attendance.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiProyectoExcel.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{AppRoles.Teacher},{AppRoles.Admin}")]
public class CoursesController(ICourseService courseService) : ControllerBase
{   
    [HttpGet]
    public async Task<IActionResult> GetCourses([FromQuery] bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var courses = await courseService.GetCoursesAsync(activeOnly, cancellationToken);
        if (User.IsInRole(AppRoles.Admin))
        {
            return Ok(courses);
        }

        var allowed = GetAllowedCourseIds();
        return Ok(courses.Where(c => allowed.Contains(c.Id)));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCourse(int id, CancellationToken cancellationToken = default)
    {
        if (!CanAccessCourse(id))
        {
            return Forbid();
        }

        var course = await courseService.GetCourseAsync(id, cancellationToken);
        if (course is null)
        {
            return NotFound(new { message = $"Course {id} not found." });
        }

        return Ok(course);
    }

    [HttpGet("{id:int}/students")]
    public async Task<IActionResult> GetStudents(int id, CancellationToken cancellationToken = default)
    {
        if (!CanAccessCourse(id))
        {
            return Forbid();
        }

        var course = await courseService.GetCourseAsync(id, cancellationToken);
        if (course is null)
        {
            return NotFound(new { message = $"Course {id} not found." });
        }

        var students = await courseService.GetStudentsByCourseAsync(id, cancellationToken);
        return Ok(students);
    }

    private bool CanAccessCourse(int courseId)
    {
        if (User.IsInRole(AppRoles.Admin))
        {
            return true;
        }

        return GetAllowedCourseIds().Contains(courseId);
    }

    private HashSet<int> GetAllowedCourseIds()
    {
        return User.FindAll("course_id")
            .Select(c => c.Value)
            .Select(v => int.TryParse(v, out var id) ? id : (int?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet();
    }
}
