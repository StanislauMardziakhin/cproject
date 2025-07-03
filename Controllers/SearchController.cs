using System.Globalization;
using System.Security.Claims;
using CourseProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CourseProject.Controllers;

[AllowAnonymous]
public class SearchController : Controller
{
    private readonly IStringLocalizer<SharedResources> _localizer;
    private readonly TemplateService _templateService;

    public SearchController(TemplateService templateService, IStringLocalizer<SharedResources> localizer)
    {
        _templateService = templateService;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index(string query, string filter = "public")
    {
        var culture = CultureInfo.CurrentUICulture.Name;
        var isAdmin = User.IsInRole("Admin");
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var templates = await _templateService.SearchAsync(query, culture, filter, userId, isAdmin);
        ViewBag.Query = query;
        ViewBag.Filter = filter;
        if (!templates.Any())
            TempData["Info"] = _localizer["NoResults"].Value;
        return View(templates);
    }
}