using CourseProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CourseProject.Controllers;

[Authorize]
public class TemplateLikesController : Controller
{
    private readonly LikeService _likeService;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public TemplateLikesController(LikeService likeService, IStringLocalizer<SharedResources> localizer)
    {
        _likeService = likeService;
        _localizer = localizer;
    }

    private string GetUserId() => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

    private IActionResult RedirectToViewWithMessage(int templateId, string messageKey, bool success)
    {
        TempData[success ? "Success" : "Error"] = _localizer[messageKey].Value;
        return RedirectToAction("View", "Templates", new { id = templateId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLike(int templateId)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return RedirectToViewWithMessage(templateId, "UserNotAuthenticated", false);
        var isLiked = await _likeService.IsLikedAsync(templateId, userId);
        var (succeeded, error) = isLiked
            ? await _likeService.RemoveLikeAsync(templateId, userId)
            : await _likeService.AddLikeAsync(templateId, userId);
        var messageKey = isLiked ? (succeeded ? "UnlikeSuccess" : error) : (succeeded ? "LikeSuccess" : error);
        return RedirectToViewWithMessage(templateId, messageKey, succeeded);
    }

    [HttpGet]
    public async Task<IActionResult> IsLiked(int templateId)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Json(false);
        var isLiked = await _likeService.IsLikedAsync(templateId, userId);
        return Json(isLiked);
    }
} 