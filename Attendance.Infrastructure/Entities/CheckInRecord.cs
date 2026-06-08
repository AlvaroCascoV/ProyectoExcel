namespace Attendance.Infrastructure.Entities;

public class CheckInRecord
{
    public int Id { get; set; }

    public int TajamarUserId { get; set; }
    public TajamarUser? TajamarUser { get; set; }

    public int DeviceId { get; set; }
    public Device? Device { get; set; }

    public int PositionId { get; set; }
    public Position? Position { get; set; }

    public DateTime CheckedInAtUtc { get; set; }

    public string? ObservedIp { get; set; }
}
