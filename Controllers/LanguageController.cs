using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Histador.Controllers;

public class LanguageController : Controller
{
    [HttpGet]
    public IActionResult Set(string culture, string? returnUrl)
    {
        if (culture != "bg" && culture != "en") culture = "bg";

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });

        if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl)) returnUrl = "/";
        return LocalRedirect(returnUrl);
    }
}
