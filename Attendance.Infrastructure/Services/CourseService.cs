using Attendance.Infrastructure.Data;
using Attendance.Infrastructure.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Attendance.Infrastructure.Services;

public interface ICourseService
{
    Task<IReadOnlyList<CourseDto>> GetCoursesAsync(bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<CourseDto?> GetCourseAsync(int courseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudentDto>> GetStudentsByCourseAsync(int courseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CourseDto>> GetCoursesForUserAsync(int userId, CancellationToken cancellationToken = default);
}

public class CourseService(ApplicationDbContext dbContext) : ICourseService
{
    public async Task<IReadOnlyList<CourseDto>> GetCoursesAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Courses.AsNoTracking();

        if (activeOnly)
        {
            query = query.Where(c => c.IsActive == true);
        }

        var courses = await query
            .OrderBy(c => c.Name)
            .Select(c => new CourseDto(
                c.Id,
                c.Name ?? string.Empty,
                c.StartDate,
                c.EndDate,
                c.IsActive == true,
                c.Enrollments.Count(e => e.User!.RoleId == 2 && e.User.IsActive == true)))
            .ToListAsync(cancellationToken);

        return courses;
    }

    public async Task<CourseDto?> GetCourseAsync(int courseId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId)
            .Select(c => new CourseDto(
                c.Id,
                c.Name ?? string.Empty,
                c.StartDate,
                c.EndDate,
                c.IsActive == true,
                c.Enrollments.Count(e => e.User!.RoleId == 2 && e.User.IsActive == true)))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StudentDto>> GetStudentsByCourseAsync(
        int courseId,
        CancellationToken cancellationToken = default)
    {
        var courseExists = await dbContext.Courses.AsNoTracking().AnyAsync(c => c.Id == courseId, cancellationToken);
        if (!courseExists)
        {
            return [];
        }

        return await dbContext.CourseEnrollments
            .AsNoTracking()
            .Where(e => e.CourseId == courseId && e.User != null && e.User.RoleId == 2)
            .OrderBy(e => e.User!.LastName)
            .ThenBy(e => e.User!.FirstName)
            .Select(e => new StudentDto(
                e.User!.Id,
                e.User.FirstName ?? string.Empty,
                e.User.LastName ?? string.Empty,
                (e.User.FirstName + " " + e.User.LastName).Trim(),
                e.User.Email,
                e.User.IsActive == true,
                e.User.ImageUrl))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CourseDto>> GetCoursesForUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.CourseEnrollments
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.Course != null)
            .OrderBy(e => e.Course!.Name)
            .Select(e => new CourseDto(
                e.Course!.Id,
                e.Course.Name ?? string.Empty,
                e.Course.StartDate,
                e.Course.EndDate,
                e.Course.IsActive == true,
                e.Course.Enrollments.Count(en => en.User!.RoleId == 2 && en.User.IsActive == true)))
            .ToListAsync(cancellationToken);
    }
}
