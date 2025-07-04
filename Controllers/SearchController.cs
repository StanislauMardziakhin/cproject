using System.Globalization;
using System.Security.Claims;
using CourseProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using CourseProject.ViewModels;

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
        var viewModels = templates.Select(t => new TemplateViewModel
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            Theme = t.Theme,
            IsPublic = t.IsPublic,
            Tags = t.Tags,
            ImageUrl = t.ImageUrl,
            LikeCount = t.Likes?.Count ?? 0,
            CommentCount = t.Comments?.Count ?? 0,
            IsLikedByCurrentUser = userId != null && t.Likes?.Any(l => l.UserId == userId) == true,
            OwnerName = t.User?.Name
        }).ToList();
        ViewBag.Query = query;
        ViewBag.Filter = filter;
        if (viewModels.Count == 0)
            TempData["Info"] = _localizer["NoResults"].Value;
        return View(viewModels);
    }
}