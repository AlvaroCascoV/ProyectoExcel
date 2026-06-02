using Attendance.Infrastructure.Data;
using Attendance.Infrastructure.DTOs;
using Attendance.Infrastructure.Entities;
using Attendance.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiProyectoExcel.Controllers;

[ApiController]
[Route("api")]
public class DevicesController(ApplicationDbContext dbContext) : ControllerBase
{
    [Authorize(Roles = $"{AppRoles.Teacher},{AppRoles.Admin}")]
    [HttpGet("devices")]
    public async Task<ActionResult<IReadOnlyList<DeviceAdminDto>>> GetDevices(CancellationToken cancellationToken = default)
    {
        var devices = await dbContext.Devices
            .AsNoTracking()
            .OrderByDescending(d => d.LastSeenAtUtc)
            .Select(d => new
            {
                d.Id,
                d.DeviceIdentifier,
                d.FriendlyName,
                d.LastSeenIp,
                d.LastSeenAtUtc,
                d.IsActive
            })
            .ToListAsync(cancellationToken);

        var currentAssignments = await dbContext.DevicePositionAssignments
            .AsNoTracking()
            .Where(a => a.IsCurrent)
            .Select(a => new { a.DeviceId, a.PositionId, a.Position!.ClassCode, a.Position.DeviceCode })
            .ToListAsync(cancellationToken);

        var byDevice = currentAssignments.ToDictionary(a => a.DeviceId);

        var result = devices.Select(d =>
        {
            byDevice.TryGetValue(d.Id, out var current);
            return new DeviceAdminDto(
                d.Id,
                d.DeviceIdentifier,
                d.FriendlyName,
                d.LastSeenIp,
                d.LastSeenAtUtc,
                d.IsActive,
                current?.PositionId,
                current?.ClassCode,
                current?.DeviceCode);
        }).ToList();

        return Ok(result);
    }

    [Authorize(Roles = $"{AppRoles.Teacher},{AppRoles.Admin}")]
    [HttpPost("devices/{deviceId:int}/assign-position")]
    public async Task<IActionResult> AssignDeviceToPosition(
        int deviceId,
        [FromBody] AssignDeviceToPositionRequest request,
        CancellationToken cancellationToken = default)
    {
        var deviceExists = await dbContext.Devices.AnyAsync(d => d.Id == deviceId, cancellationToken);
        if (!deviceExists)
        {
            return NotFound(new { message = $"Device {deviceId} not found." });
        }

        var position = await dbContext.Positions.FirstOrDefaultAsync(p => p.Id == request.PositionId, cancellationToken);
        if (position is null)
        {
            return NotFound(new { message = $"Position {request.PositionId} not found." });
        }

        var now = DateTime.UtcNow;

        var currentForDevice = await dbContext.DevicePositionAssignments
            .Where(a => a.DeviceId == deviceId && a.IsCurrent)
            .ToListAsync(cancellationToken);

        foreach (var a in currentForDevice)
        {
            a.IsCurrent = false;
            a.UnassignedAtUtc = now;
        }

        var currentForPosition = await dbContext.DevicePositionAssignments
            .Where(a => a.PositionId == request.PositionId && a.IsCurrent)
            .ToListAsync(cancellationToken);

        foreach (var a in currentForPosition)
        {
            a.IsCurrent = false;
            a.UnassignedAtUtc = now;
        }

        dbContext.DevicePositionAssignments.Add(new DevicePositionAssignment
        {
            DeviceId = deviceId,
            PositionId = request.PositionId,
            AssignedAtUtc = now,
            IsCurrent = true
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

