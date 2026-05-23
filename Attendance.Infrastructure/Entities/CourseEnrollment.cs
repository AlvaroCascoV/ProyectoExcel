namespace Attendance.Infrastructure.Entities;

public class CourseEnrollment
{
    public int Id { get; set; }
    public int? CourseId { get; set; }
    public int? UserId { get; set; }

    public Course? Course { get; set; }
    public TajamarUser? User { get; set; }
}
