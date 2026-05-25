using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MvcProyectoExcel.Controllers;

public class CultureController : Controller
{
    private readonly IOptions<RequestLocalizationOptions> _locOptions;

    public CultureController(IOptions<RequestLocalizationOptions> locOptions)
    {
        _locOptions = locOptions;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetCulture(string culture, string returnUrl)
    {
        var supportedCultures = _locOptions.Value.SupportedUICultures;
        if (!string.IsNullOrWhiteSpace(culture) &&
            supportedCultures != null &&
            supportedCultures.Any(c => c.Name == culture))
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );
        }

        var safeUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : "/";
        return LocalRedirect(safeUrl);
    }
}
