using System.Security.Claims;
using System.Text.Json;
using Attendance.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using MvcProyectoExcel.Services;
using MvcProyectoExcel.ViewModels;

namespace MvcProyectoExcel.Controllers;

[Authorize(Roles = AppRoles.Student)]
public class CheckInController(IAttendanceApiClient apiClient, IStringLocalizer<SharedResource> localizer) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await BuildModelAsync(cancellationToken);

        try
        {
            await apiClient.RegisterDeviceAsync(cancellationToken);
            model.Context = await apiClient.GetCheckInContextAsync(cancellationToken);
            return View(model);
        }
        catch (HttpRequestException ex)
        {
            model.ErrorMessage = GetApiErrorMessage(ex, model);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoCheckIn(CancellationToken cancellationToken)
    {
        var model = await BuildModelAsync(cancellationToken);

        try
        {
            await apiClient.RegisterDeviceAsync(cancellationToken);
            model.Context = await apiClient.GetCheckInContextAsync(cancellationToken);

            if (model.IsAssignedToAnotherStudent)
            {
                model.ErrorMessage = GetWrongStudentMessage(model);
                return View("Index", model);
            }

            model.LastCheckIn = await apiClient.CheckInAsync(cancellationToken);
            model.Context = await apiClient.GetCheckInContextAsync(cancellationToken);
            return View("Index", model);
        }
        catch (HttpRequestException ex)
        {
            model.ErrorMessage = GetApiErrorMessage(ex, model);
            return View("Index", model);
        }
    }

    private Task<CheckInViewModel> BuildModelAsync(CancellationToken cancellationToken)
    {
        EnsureDeviceCookie();

        return Task.FromResult(new CheckInViewModel
        {
            DeviceIdentifier = Request.Cookies[DeviceCookieNames.DeviceId] ?? string.Empty,
            CurrentTajamarUserId = int.TryParse(User.FindFirstValue("tajamar_user_id"), out var id) ? id : null,
            CurrentUserFullName = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty
        });
    }

    private string GetApiErrorMessage(HttpRequestException ex, CheckInViewModel model)
    {
        var apiMessage = TryParseApiMessage(ex.Message);
        if (apiMessage is "CHECKIN_WRONG_STUDENT"
            || apiMessage.Contains("different student", StringComparison.OrdinalIgnoreCase))
        {
            return GetWrongStudentMessage(model);
        }

        if (apiMessage is "CHECKIN_BLOCKED_ABSENT")
        {
            return localizer["CheckInBlockedAbsent"];
        }

        if (apiMessage.Contains("not assigned to a position", StringComparison.OrdinalIgnoreCase))
        {
            return localizer["PositionNotAssigned"];
        }

        if (apiMessage.Contains("not assigned to a student", StringComparison.OrdinalIgnoreCase))
        {
            return localizer["StudentNotAssigned"];
        }

        if (ex.Message.Contains("Connection refused", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("No connection could be made", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("Name or service not known", StringComparison.OrdinalIgnoreCase))
        {
            return localizer["ErrorApiUnreachable"];
        }

        return string.IsNullOrWhiteSpace(apiMessage) ? localizer["CheckInFailed"] : apiMessage;
    }

    private string GetWrongStudentMessage(CheckInViewModel model)
    {
        var assigned = model.Context?.AssignedStudentFullName;
        if (string.IsNullOrWhiteSpace(assigned) && model.Context?.AssignedTajamarUserId is not null)
        {
            assigned = $"ID {model.Context.AssignedTajamarUserId}";
        }

        return localizer["CheckInWrongStudent", assigned ?? "?", model.CurrentUserFullName, model.CurrentTajamarUserId ?? 0];
    }

    private static string TryParseApiMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return string.Empty;
        }

        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("message", out var message))
            {
                return message.GetString() ?? body;
            }
        }
        catch (JsonException)
        {
            // Not JSON — return raw body.
        }

        return body;
    }

    private void EnsureDeviceCookie()
    {
        var existing = Request.Cookies[DeviceCookieNames.DeviceId];
        if (!string.IsNullOrWhiteSpace(existing))
        {
            return;
        }

        var deviceId = Guid.NewGuid().ToString();
        Response.Cookies.Append(DeviceCookieNames.DeviceId, deviceId, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddYears(5)
        });
    }
}
