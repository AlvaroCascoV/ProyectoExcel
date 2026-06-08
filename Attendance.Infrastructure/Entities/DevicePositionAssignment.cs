namespace Attendance.Infrastructure.Entities;

public class DevicePositionAssignment
{
    public int Id { get; set; }

    public int DeviceId { get; set; }
    public Device? Device { get; set; }

    public int PositionId { get; set; }
    public Position? Position { get; set; }

    public DateTime AssignedAtUtc { get; set; }

    public DateTime? UnassignedAtUtc { get; set; }

    public bool IsCurrent { get; set; }
}
