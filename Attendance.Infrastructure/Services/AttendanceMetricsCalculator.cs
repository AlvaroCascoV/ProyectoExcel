using Attendance.Infrastructure.Entities;

namespace Attendance.Infrastructure.Services;

public static class AttendanceMetricsCalculator
{
    public const decimal DiplomaThreshold = 80m;
    public const decimal DropThreshold = 75m;

    public record Metrics(
        int PresentCount,
        int AbsentCount,
        int LateCount,
        int JustifiedAbsentCount,
        int JustifiedLateCount,
        int EarlyLeaveCount,
        int JustifiedEarlyLeaveCount,
        decimal UnjustifiedWeighted,
        decimal AbsentFOnly,
        decimal AbsentFAndR,
        decimal AttendancePercentage,
        decimal RealAttendancePercentage,
        decimal AbsentFPercentage,
        decimal AbsentFRPercentage,
        bool DiplomaEligible,
        bool AtRiskDrop);

    public static Metrics FromStatuses(IReadOnlyList<AttendanceStatus> statuses, int totalClassDays)
    {
        var present = statuses.Count(s => s == AttendanceStatus.Present);
        var absent = statuses.Count(s => s == AttendanceStatus.Absent);
        var late = statuses.Count(s => s == AttendanceStatus.Late);
        var justifiedAbsent = statuses.Count(s => s == AttendanceStatus.JustifiedAbsent);
        var justifiedLate = statuses.Count(s => s == AttendanceStatus.JustifiedLate);
        var earlyLeave = statuses.Count(s => s == AttendanceStatus.EarlyLeave);
        var justifiedEarlyLeave = statuses.Count(s => s == AttendanceStatus.JustifiedEarlyLeave);

        var unjustifiedWeighted = absent + (late * 0.5m) + (earlyLeave * 0.5m);
        var absentFOnly = justifiedAbsent + absent;
        var absentFAndR = absent + late + earlyLeave + justifiedAbsent + justifiedLate + justifiedEarlyLeave;

        var totalDays = totalClassDays > 0 ? totalClassDays : statuses.Count;

        decimal PercentFromAbsenceUnits(decimal absenceUnits) =>
            totalDays == 0 ? 100m : Math.Round(100m - (absenceUnits * 100m / totalDays), 1);

        var attended = present + late + justifiedLate + earlyLeave + justifiedEarlyLeave;
        var attendancePercentage = totalDays == 0 ? 100m : Math.Round(attended * 100m / totalDays, 1);
        var realAttendancePercentage = PercentFromAbsenceUnits(unjustifiedWeighted);

        return new Metrics(
            present,
            absent,
            late,
            justifiedAbsent,
            justifiedLate,
            earlyLeave,
            justifiedEarlyLeave,
            unjustifiedWeighted,
            absentFOnly,
            absentFAndR,
            attendancePercentage,
            realAttendancePercentage,
            PercentFromAbsenceUnits(absentFOnly),
            PercentFromAbsenceUnits(absentFAndR),
            realAttendancePercentage >= DiplomaThreshold,
            realAttendancePercentage < DropThreshold);
    }
}
