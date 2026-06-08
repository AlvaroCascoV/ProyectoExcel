using Attendance.Infrastructure.DTOs;
using Attendance.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiProyectoExcel.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{AppRoles.Teacher},{AppRoles.Admin}")]
public class StudentsController(ICourseService courseService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StudentDto>>> GetStudents(CancellationToken cancellationToken = default)
    {
        var students = await courseService.GetStudentsAsync(cancellationToken);
        return Ok(students);
    }
}

