using Microsoft.AspNetCore.Identity;

namespace Attendance.Infrastructure.Entities;

public class ApplicationUser : IdentityUser
{
    public int TajamarUserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
