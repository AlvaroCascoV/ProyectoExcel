using Attendance.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using MvcProyectoExcel.Services;

namespace MvcProyectoExcel.Controllers;

[Authorize(Roles = $"{AppRoles.Teacher},{AppRoles.Admin}")]
public class DevicesController(IAttendanceApiClient apiClient, IStringLocalizer<SharedResource> localizer) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            var devices = await apiClient.GetDevicesAdminAsync(cancellationToken);
            return View(devices);
        }
        catch (HttpRequestException)
        {
            ViewBag.ErrorMessage = localizer["ErrorApiUnreachable"].Value;
            return View(Array.Empty<Attendance.Infrastructure.DTOs.DeviceAdminDto>());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(int deviceId, int positionId, CancellationToken cancellationToken)
    {
        try
        {
            await apiClient.AssignDeviceToPositionAsync(deviceId, positionId, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] = localizer["ErrorApiUnreachable"].Value;
            return RedirectToAction(nameof(Index));
        }
    }
}

