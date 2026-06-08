using System.Security.Claims;
using Attendance.Infrastructure.DTOs;
using Attendance.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiProyectoExcel.Controllers;

[ApiController]
[Route("api")]
public class CheckInsController(ICheckInService checkInService) : ControllerBase
{
    [Authorize]
    [HttpPost("devices/register")]
    public async Task<ActionResult<RegisterDeviceResponse>> RegisterDevice(CancellationToken cancellationToken = default)
    {
        if (!TryGetDeviceIdentifier(out var deviceIdentifier))
        {
            return BadRequest(new { message = $"Missing header '{DeviceHeaders.DeviceIdentifier}'." });
        }

        var ip = GetObservedIp();
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await checkInService.RegisterOrTouchDeviceAsync(
            deviceIdentifier,
            ip,
            userAgent,
            cancellationToken);

        return Ok(result);
    }

    [Authorize]
    [HttpGet("checkins/context")]
    public async Task<ActionResult<CheckInContextResponse>> GetContext(CancellationToken cancellationToken = default)
    {
        if (!TryGetDeviceIdentifier(out var deviceIdentifier))
        {
            return BadRequest(new { message = $"Missing header '{DeviceHeaders.DeviceIdentifier}'." });
        }

        var context = await checkInService.GetContextAsync(deviceIdentifier, cancellationToken);
        if (context is null)
        {
            // Allow the client to call /devices/register to create it.
            return NotFound(new { message = "Device not found." });
        }

        return Ok(context);
    }

    [Authorize(Roles = AppRoles.Student)]
    [HttpPost("checkins")]
    public async Task<ActionResult<CheckInResponse>> CheckIn(CancellationToken cancellationToken = default)
    {
        if (!TryGetTajamarUserId(out var tajamarUserId))
        {
            return Unauthorized(new { message = "Could not resolve the current user." });
        }

        if (!TryGetDeviceIdentifier(out var deviceIdentifier))
        {
            return BadRequest(new { message = $"Missing header '{DeviceHeaders.DeviceIdentifier}'." });
        }

        var ip = GetObservedIp();
        var (success, error, response) = await checkInService.CheckInAsync(
            tajamarUserId,
            deviceIdentifier,
            ip,
            cancellationToken);

        if (!success || response is null)
        {
            return BadRequest(new { message = error ?? "Check-in failed." });
        }

        return Ok(response);
    }

    private bool TryGetTajamarUserId(out int userId)
    {
        var claim = User.FindFirstValue("tajamar_user_id");
        return int.TryParse(claim, out userId);
    }

    private bool TryGetDeviceIdentifier(out string deviceIdentifier)
    {
        deviceIdentifier = Request.Headers[DeviceHeaders.DeviceIdentifier].ToString().Trim();
        if (string.IsNullOrWhiteSpace(deviceIdentifier))
        {
            return false;
        }

        // Basic sanity for GUID-based identifiers (still allow custom identifiers).
        if (deviceIdentifier.Length > 64)
        {
            deviceIdentifier = deviceIdentifier[..64];
        }

        return true;
    }

    private string? GetObservedIp()
    {
        var forwarded = Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrWhiteSpace(forwarded))
        {
            return forwarded.Split(',')[0].Trim();
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}

