using Attendance.Infrastructure.DTOs;

namespace MvcProyectoExcel.ViewModels;

public class CheckInViewModel
{
    public string DeviceIdentifier { get; set; } = string.Empty;

    public CheckInContextResponse? Context { get; set; }

    public CheckInResponse? LastCheckIn { get; set; }

    public string? ErrorMessage { get; set; }

    public int? CurrentTajamarUserId { get; set; }

    public string CurrentUserFullName { get; set; } = string.Empty;

    public bool IsAssignedToAnotherStudent =>
        Context?.AssignedTajamarUserId is not null
        && CurrentTajamarUserId.HasValue
        && Context.AssignedTajamarUserId != CurrentTajamarUserId;
}

