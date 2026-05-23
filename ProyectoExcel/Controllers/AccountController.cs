using System.Security.Claims;
using Attendance.Infrastructure.DTOs;
using Attendance.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcProyectoExcel.Services;
using MvcProyectoExcel.ViewModels;

namespace MvcProyectoExcel.Controllers;

public class AccountController(IAttendanceApiClient apiClient) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var response = await apiClient.LoginAsync(new LoginRequest(model.Email, model.Password), cancellationToken);
        if (response is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, response.Email),
            new(ClaimTypes.Name, response.FullName),
            new(ClaimTypes.Role, response.Role),
            new("tajamar_user_id", response.TajamarUserId.ToString())
        };

        foreach (var courseId in response.CourseIds)
        {
            claims.Add(new Claim("course_id", courseId.ToString()));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = response.ExpiresAt
            });

        Response.Cookies.Append(AuthCookieNames.Jwt, response.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = response.ExpiresAt
        });

        return RedirectToLocal(model.ReturnUrl, response.Role);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        Response.Cookies.Delete(AuthCookieNames.Jwt);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private IActionResult RedirectToLocal(string? returnUrl, string? role = null)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        role ??= User.FindFirstValue(ClaimTypes.Role);

        return role switch
        {
            AppRoles.Student => RedirectToAction("Index", "Dashboard"),
            AppRoles.Teacher or AppRoles.Admin => RedirectToAction("Index", "Attendance"),
            _ => RedirectToAction("Index", "Home")
        };
    }
}
