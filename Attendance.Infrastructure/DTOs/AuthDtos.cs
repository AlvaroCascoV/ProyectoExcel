namespace Attendance.Infrastructure.DTOs;

public record LoginRequest(string Email, string Password);

public record LoginResponse(
    string Token,
    string Email,
    string Role,
    int TajamarUserId,
    string FullName,
    IReadOnlyList<int> CourseIds,
    DateTime ExpiresAt);

public record AuthUserDto(
    string Id,
    string Email,
    string Role,
    int TajamarUserId,
    string FullName,
    IReadOnlyList<int> CourseIds);
