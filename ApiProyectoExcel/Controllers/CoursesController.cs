using Attendance.Infrastructure.DTOs;
using Attendance.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        return Ok(courses);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCourse(int id, CancellationToken cancellationToken = default)
    {
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
        var course = await courseService.GetCourseAsync(id, cancellationToken);
        if (course is null)
        {
            return NotFound(new { message = $"Course {id} not found." });
        }

        var students = await courseService.GetStudentsByCourseAsync(id, cancellationToken);
        return Ok(students);
    }
}
