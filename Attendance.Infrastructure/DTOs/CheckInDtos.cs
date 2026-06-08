namespace Attendance.Infrastructure.DTOs;

public static class DeviceHeaders
{
    public const string DeviceIdentifier = "X-Device-Identifier";
}

public sealed record RegisterDeviceResponse(
    int DeviceId,
    string DeviceIdentifier,
    string? LastSeenIp,
    DateTime LastSeenAtUtc);

public sealed record CheckInResponse(
    int CheckInId,
    int TajamarUserId,
    int DeviceId,
    int PositionId,
    string ClassCode,
    string DeviceCode,
    DateTime CheckedInAtUtc);

public sealed record CheckInContextResponse(
    int DeviceId,
    string DeviceIdentifier,
    string? FriendlyName,
    string? LastSeenIp,
    DateTime LastSeenAtUtc,
    int? PositionId,
    string? ClassCode,
    string? DeviceCode,
    int? AssignedTajamarUserId,
    string? AssignedStudentFullName);

public sealed record DeviceAdminDto(
    int DeviceId,
    string DeviceIdentifier,
    string? FriendlyName,
    string? LastSeenIp,
    DateTime LastSeenAtUtc,
    bool IsActive,
    int? CurrentPositionId,
    string? ClassCode,
    string? DeviceCode);

public sealed record PositionAdminDto(
    int PositionId,
    string ClassCode,
    string DeviceCode,
    bool IsActive,
    int? CurrentDeviceId,
    int? CurrentTajamarUserId);

public sealed record AssignDeviceToPositionRequest(int PositionId);

public sealed record AssignPositionToUserRequest(int TajamarUserId);

