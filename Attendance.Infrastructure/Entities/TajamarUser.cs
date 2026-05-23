namespace Attendance.Infrastructure.Entities;

public class TajamarUser
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public bool? IsActive { get; set; }
    public string? ImageUrl { get; set; }
    public string? LegacyPassword { get; set; }
    public int? RoleId { get; set; }

    public RoleTajamar? Role { get; set; }
    public ICollection<CourseEnrollment> Enrollments { get; set; } = [];
    public ICollection<AttendanceRecord> AttendanceAsStudent { get; set; } = [];
    public ICollection<AttendanceRecord> AttendanceAsTeacher { get; set; } = [];
}
