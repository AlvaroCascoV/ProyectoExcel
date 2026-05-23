namespace Attendance.Infrastructure.Entities;

public class RoleTajamar
{
    public int Id { get; set; }
    public string? Name { get; set; }

    public ICollection<TajamarUser> Users { get; set; } = [];
}
