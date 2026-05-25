using System.ComponentModel.DataAnnotations;
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
    [Range(1, int.MaxValue, ErrorMessage = "StudentId must be a positive integer.")]
    int StudentId,

    [EnumDataType(typeof(AttendanceStatus), ErrorMessage = "Invalid attendance status.")]
    AttendanceStatus Status,

    [MaxLength(500, ErrorMessage = "Comment cannot exceed 500 characters.")]
    string? Comment);

public record SaveAttendanceSessionRequest(
    [Required(ErrorMessage = "Entries list is required.")]
    [MinLength(1, ErrorMessage = "At least one attendance entry is required.")]
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
