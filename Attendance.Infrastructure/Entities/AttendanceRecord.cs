namespace Attendance.Infrastructure.Entities;

public class AttendanceRecord
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateOnly Date { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Comment { get; set; }
    public int RecordedByUserId { get; set; }
    public DateTime RecordedAt { get; set; }

    public TajamarUser? Student { get; set; }
    public Course? Course { get; set; }
    public TajamarUser? RecordedBy { get; set; }
}
