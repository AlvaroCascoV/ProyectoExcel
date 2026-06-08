namespace Attendance.Infrastructure.Entities;

public class PositionUserAssignment
{
    public int Id { get; set; }

    public int PositionId { get; set; }
    public Position? Position { get; set; }

    public int TajamarUserId { get; set; }
    public TajamarUser? TajamarUser { get; set; }

    public DateTime AssignedAtUtc { get; set; }

    public DateTime? UnassignedAtUtc { get; set; }

    public bool IsCurrent { get; set; }
}
