namespace Attendance.Infrastructure.Entities;

public class CourseCalendarEntry
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public DateOnly Date { get; set; }
    public bool IsLective { get; set; }
    public string? DayType { get; set; }
    public string? Module { get; set; }
    public string? Teacher { get; set; }
    public string? Room { get; set; }
    public DateTime UploadedAt { get; set; }

    public Course? Course { get; set; }
}
