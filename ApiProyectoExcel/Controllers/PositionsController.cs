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
public class PositionsController(ApplicationDbContext dbContext) : ControllerBase
{
    [Authorize(Roles = $"{AppRoles.Teacher},{AppRoles.Admin}")]
    [HttpGet("positions")]
    public async Task<ActionResult<IReadOnlyList<PositionAdminDto>>> GetPositions(CancellationToken cancellationToken = default)
    {
        var positions = await dbContext.Positions
            .AsNoTracking()
            .OrderBy(p => p.ClassCode)
            .ThenBy(p => p.DeviceCode)
            .Select(p => new { p.Id, p.ClassCode, p.DeviceCode, p.IsActive })
            .ToListAsync(cancellationToken);

        var currentDevices = await dbContext.DevicePositionAssignments
            .AsNoTracking()
            .Where(a => a.IsCurrent)
            .Select(a => new { a.PositionId, a.DeviceId })
            .ToListAsync(cancellationToken);

        var currentUsers = await dbContext.PositionUserAssignments
            .AsNoTracking()
            .Where(a => a.IsCurrent)
            .Select(a => new { a.PositionId, a.TajamarUserId })
            .ToListAsync(cancellationToken);

        var byPositionDevice = currentDevices.ToDictionary(x => x.PositionId, x => (int?)x.DeviceId);
        var byPositionUser = currentUsers.ToDictionary(x => x.PositionId, x => (int?)x.TajamarUserId);

        var result = positions.Select(p =>
        {
            byPositionDevice.TryGetValue(p.Id, out var deviceId);
            byPositionUser.TryGetValue(p.Id, out var userId);
            return new PositionAdminDto(
                p.Id,
                p.ClassCode,
                p.DeviceCode,
                p.IsActive,
                deviceId,
                userId);
        }).ToList();

        return Ok(result);
    }

    [Authorize(Roles = $"{AppRoles.Teacher},{AppRoles.Admin}")]
    [HttpPost("positions/{positionId:int}/assign-user")]
    public async Task<IActionResult> AssignPositionToUser(
        int positionId,
        [FromBody] AssignPositionToUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var positionExists = await dbContext.Positions.AnyAsync(p => p.Id == positionId, cancellationToken);
        if (!positionExists)
        {
            return NotFound(new { message = $"Position {positionId} not found." });
        }

        var userExists = await dbContext.TajamarUsers.AnyAsync(u => u.Id == request.TajamarUserId, cancellationToken);
        if (!userExists)
        {
            return NotFound(new { message = $"User {request.TajamarUserId} not found." });
        }

        var now = DateTime.UtcNow;

        var current = await dbContext.PositionUserAssignments
            .Where(a => a.PositionId == positionId && a.IsCurrent)
            .ToListAsync(cancellationToken);

        foreach (var a in current)
        {
            a.IsCurrent = false;
            a.UnassignedAtUtc = now;
        }

        dbContext.PositionUserAssignments.Add(new PositionUserAssignment
        {
            PositionId = positionId,
            TajamarUserId = request.TajamarUserId,
            AssignedAtUtc = now,
            IsCurrent = true
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = $"{AppRoles.Teacher},{AppRoles.Admin}")]
    [HttpPost("positions/seed")]
    public async Task<IActionResult> SeedPositions(
        [FromQuery] string classCode = "T38",
        [FromQuery] int from = 1,
        [FromQuery] int to = 25,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(classCode) || classCode.Length > 16)
        {
            return BadRequest(new { message = "Invalid classCode." });
        }

        if (from < 1 || to < from || to > 200)
        {
            return BadRequest(new { message = "Invalid range." });
        }

        classCode = classCode.Trim().ToUpperInvariant();

        var existing = await dbContext.Positions
            .AsNoTracking()
            .Where(p => p.ClassCode == classCode)
            .Select(p => p.DeviceCode)
            .ToHashSetAsync(cancellationToken);

        var created = 0;
        for (var i = from; i <= to; i++)
        {
            var deviceCode = $"W{i:00}";
            if (existing.Contains(deviceCode))
            {
                continue;
            }

            dbContext.Positions.Add(new Position
            {
                ClassCode = classCode,
                DeviceCode = deviceCode,
                IsActive = true
            });
            created++;
        }

        if (created > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Ok(new { classCode, from, to, created });
    }
}

