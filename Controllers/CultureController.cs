using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace CourseProject.Controllers;

[Route("Culture")]
public class CultureController : Controller
{
    [HttpPost]
    [Route("SetCulture")]
    public IActionResult SetCulture(string culture, string returnUrl)
    {
        if (!string.IsNullOrEmpty(culture))
        {
            Response.Cookies.Append(
                "UserCulture",
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );
        }
        return LocalRedirect(returnUrl ?? "~/");
    }
}