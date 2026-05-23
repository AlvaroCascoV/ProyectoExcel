namespace Attendance.Infrastructure.DTOs;

public record CourseDto(
    int Id,
    string Name,
    DateTime? StartDate,
    DateTime? EndDate,
    bool IsActive,
    int StudentCount);

public record StudentDto(
    int Id,
    string FirstName,
    string LastName,
    string FullName,
    string? Email,
    bool IsActive,
    string? ImageUrl);
