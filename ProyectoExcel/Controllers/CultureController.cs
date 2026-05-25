using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace MvcProyectoExcel.Controllers;

public class CultureController : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetCulture(string culture, string returnUrl)
    {
        if (culture != null)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );
        }

        return LocalRedirect(returnUrl ?? "/");
    }
}
