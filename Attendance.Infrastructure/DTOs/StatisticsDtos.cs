namespace Attendance.Infrastructure.DTOs;

public record StudentStatisticsDto(
    int StudentId,
    string FullName,
    int PresentCount,
    int AbsentCount,
    int LateCount,
    int JustifiedAbsentCount,
    int JustifiedLateCount,
    int EarlyLeaveCount,
    int JustifiedEarlyLeaveCount,
    decimal UnjustifiedWeighted,
    decimal RealAttendancePercentage,
    decimal AbsentFPercentage,
    decimal AbsentFRPercentage,
    bool DiplomaEligible,
    bool AtRiskDrop,
    int Rank);

public record CourseStatisticsDto(
    int CourseId,
    string CourseName,
    int TotalClassDays,
    decimal AverageRealAttendancePercentage,
    int AtRiskCount,
    int BelowDropThresholdCount,
    IReadOnlyList<StudentStatisticsDto> Students);

public record RankingEntryDto(
    int Rank,
    int StudentId,
    string FullName,
    decimal RealAttendancePercentage,
    decimal UnjustifiedWeighted,
    bool DiplomaEligible);
