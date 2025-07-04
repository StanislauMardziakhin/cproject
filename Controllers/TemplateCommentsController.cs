using CourseProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CourseProject.Controllers;

[Authorize]
public class TemplateCommentsController : Controller
{
    private readonly CommentService _commentService;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public TemplateCommentsController(CommentService commentService, IStringLocalizer<SharedResources> localizer)
    {
        _commentService = commentService;
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
    public async Task<IActionResult> AddComment(int templateId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return RedirectToViewWithMessage(templateId, "CommentRequired", false);
        var (succeeded, error) = await _commentService.AddCommentAsync(templateId, content, GetUserId());
        return RedirectToViewWithMessage(templateId, succeeded ? "SuccessCommentAdded" : error, succeeded);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteComment(int commentId, int templateId)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return RedirectToViewWithMessage(templateId, "UserNotAuthenticated", false);
        var (succeeded, error) = await _commentService.DeleteCommentAsync(commentId, userId);
        return RedirectToViewWithMessage(templateId, succeeded ? "CommentDeleted" : error, succeeded);
    }
} 