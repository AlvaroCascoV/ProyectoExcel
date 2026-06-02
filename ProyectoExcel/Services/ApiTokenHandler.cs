using MvcProyectoExcel.Services;
using Attendance.Infrastructure.DTOs;

namespace MvcProyectoExcel.Services;

public class ApiTokenHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var token = httpContext?.Request.Cookies[AuthCookieNames.Jwt];
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var deviceId = httpContext?.Request.Cookies[DeviceCookieNames.DeviceId];
        if (!string.IsNullOrWhiteSpace(deviceId))
        {
            request.Headers.Remove(DeviceHeaders.DeviceIdentifier);
            request.Headers.Add(DeviceHeaders.DeviceIdentifier, deviceId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
