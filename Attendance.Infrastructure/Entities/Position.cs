namespace Attendance.Infrastructure.Entities;

public class Position
{
    public int Id { get; set; }

    public string ClassCode { get; set; } = string.Empty;

    public string DeviceCode { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<DevicePositionAssignment> DeviceAssignments { get; set; } = new List<DevicePositionAssignment>();

    public ICollection<PositionUserAssignment> UserAssignments { get; set; } = new List<PositionUserAssignment>();

    public ICollection<CheckInRecord> CheckIns { get; set; } = new List<CheckInRecord>();
}
