using System.ComponentModel.DataAnnotations;

namespace Attendance.Infrastructure.DTOs;

public record LoginRequest(
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "A valid email address is required.")]
    string Email,

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(1, ErrorMessage = "Password cannot be empty.")]
    string Password);

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
