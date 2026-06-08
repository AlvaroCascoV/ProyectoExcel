namespace Attendance.Infrastructure.Entities;

public class Device
{
    public int Id { get; set; }

    public string DeviceIdentifier { get; set; } = string.Empty;

    public DateTime FirstSeenAtUtc { get; set; }

    public DateTime LastSeenAtUtc { get; set; }

    public string? LastSeenIp { get; set; }

    public string? LastSeenUserAgent { get; set; }

    public string? FriendlyName { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<DevicePositionAssignment> PositionAssignments { get; set; } = new List<DevicePositionAssignment>();

    public ICollection<CheckInRecord> CheckIns { get; set; } = new List<CheckInRecord>();
}
