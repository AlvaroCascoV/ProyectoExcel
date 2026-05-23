using Attendance.Infrastructure.Entities;

namespace Attendance.Infrastructure.DTOs;

public record AttendanceEntryDto(
    int Id,
    int StudentId,
    string StudentFullName,
    AttendanceStatus Status,
    string? Comment);

public record AttendanceSessionDto(
    int CourseId,
    string CourseName,
    DateOnly Date,
    IReadOnlyList<AttendanceEntryDto> Entries);

public record SaveAttendanceEntryRequest(
    int StudentId,
    AttendanceStatus Status,
    string? Comment);

public record SaveAttendanceSessionRequest(
    IReadOnlyList<SaveAttendanceEntryRequest> Entries);

public record SeedAttendanceResultDto(
    int CourseId,
    int DaysSeeded,
    int StudentsAffected,
    int RecordsUpserted,
    IReadOnlyList<DateOnly> Dates);

public record AttendanceRecordDto(
    int Id,
    int CourseId,
    string CourseName,
    DateOnly Date,
    AttendanceStatus Status,
    string? Comment,
    DateTime RecordedAt);

public record AttendanceSummaryDto(
    int CourseId,
    string CourseName,
    int TotalSessions,
    int PresentCount,
    int AbsentCount,
    int LateCount,
    int JustifiedAbsentCount,
    int JustifiedLateCount,
    int EarlyLeaveCount,
    int JustifiedEarlyLeaveCount,
    decimal AttendancePercentage,
    decimal RealAttendancePercentage,
    bool DiplomaEligible,
    bool AtRiskDrop);
