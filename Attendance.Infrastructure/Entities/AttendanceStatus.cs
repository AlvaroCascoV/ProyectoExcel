namespace Attendance.Infrastructure.Entities;

public enum AttendanceStatus : byte
{
    Present = 0,
    Absent = 1,
    Late = 2,
    JustifiedAbsent = 3,
    JustifiedLate = 4,
    EarlyLeave = 5,
    JustifiedEarlyLeave = 6
}
