namespace Attendance.Infrastructure.Entities;

public class Course
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsActive { get; set; }

    public ICollection<CourseEnrollment> Enrollments { get; set; } = [];
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = [];
    public ICollection<CourseCalendarEntry> CalendarEntries { get; set; } = [];
}
