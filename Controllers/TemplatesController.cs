using System.Security.Claims;
using CourseProject.Models;
using CourseProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CourseProject.Controllers;

[Authorize]
public class TemplatesController : Controller
{
    private readonly IStringLocalizer<SharedResources> _localizer;
    private readonly QuestionService _questionService;
    private readonly TemplateService _templateService;
    private readonly FormService _formService;
    private readonly IFormResultService _formResultService;
    private readonly IResultsAggregatorService _resultsAggregatorService;
    private readonly CommentService _commentService;
    private readonly LikeService _likeService;

    public TemplatesController(TemplateService templateService, IStringLocalizer<SharedResources> localizer,
        QuestionService questionService, FormService formService, IFormResultService formResultService,
        IResultsAggregatorService resultsAggregatorService, CommentService commentService,
        LikeService likeService)
    {
        _templateService = templateService;
        _localizer = localizer;
        _questionService = questionService;
        _formService = formService;
        _formResultService = formResultService;
        _resultsAggregatorService = resultsAggregatorService;
        _commentService = commentService;
        _likeService = likeService;
    }

    private string GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }

    public async Task<IActionResult> Index(bool? publicOnly = null)
    {
        var userId = User?.Identity?.IsAuthenticated == true ? GetUserId() : null;
        var templates = await _templateService.GetUserTemplatesAsync(userId, publicOnly);
        return View(templates);
    }

    public IActionResult Create()
    {
        return View(new Template());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Template template, IFormFile? image)
    {
        if (!ModelState.IsValid) return HandleError(template, "TErrorInvalidInput");
        var (succeeded, error) = await _templateService.CreateAsync(template, image, GetUserId());
        return succeeded ? HandleSuccess("SuccessTemplateCreated") : HandleError(template, error);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var userId = User.Identity?.IsAuthenticated == true ? GetUserId() : null;
        var isAdmin = User.IsInRole("Admin");
        var template = await _templateService.GetForEditAsync(id, userId, isAdmin);
        if (template == null) return NotFound();
        template.Forms = await _formService.GetFormsForTemplateAsync(id, userId, isAdmin);
        var aggregatedResults = await _resultsAggregatorService.GetAggregatedResultsAsync(id);
        ViewData["AggregatedResults"] = aggregatedResults;
        return View(template);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Template template, IFormFile? image)
    {
        if (id != template.Id || !ModelState.IsValid) return HandleError(template, "TErrorInvalidInput");
        var isAdmin = User.IsInRole("Admin");
        var (succeeded, error) = await _templateService.UpdateAsync(id, template, image, GetUserId(), isAdmin);
        return succeeded ? HandleSuccess("SuccessTemplateUpdated") : HandleError(template, error);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (succeeded, error) = await _templateService.DeleteAsync(id, GetUserId());
        return succeeded ? HandleSuccess("SuccessTemplateDeleted") : HandleError(null, error);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyAction(string action, string templateIds)
    {
        if (!Enum.TryParse<TemplateActionType>(action, true, out _))
        {
            TempData["Error"] = _localizer["ErrorInvalidAction"].Value;
            return RedirectToAction(nameof(Index));
        }

        var (succeeded, ids, errorKey) = ParseTemplateIds(templateIds);
        if (!succeeded)
        {
            TempData["Error"] = _localizer[errorKey].Value;
            return RedirectToAction(nameof(Index));
        }

        var userId = GetUserId();
        var isAdmin = User.IsInRole("Admin");
        var successMessageKey = action.ToLower() switch
        {
            "delete" => "SuccessDeleteTemplate",
            "publish" => "SuccessPublishTemplate",
            "unpublish" => "SuccessUnpublishTemplate",
            _ => "ErrorInvalidAction"
        };
        var (actionSucceeded, error) =
            await _templateService.ApplyMassActionAsync(action.ToLower(), ids, userId, isAdmin);
        return actionSucceeded ? HandleSuccess(successMessageKey) : HandleError(null, error);
    }

    private (bool succeeded, int[] ids, string errorKey) ParseTemplateIds(string templateIds)
    {
        if (string.IsNullOrEmpty(templateIds))
            return (false, [], "ErrorInvalidAction");
        try
        {
            var ids = templateIds.Split(',').Select(int.Parse).ToArray();
            return ids.Any() ? (true, ids, "") : (false, [], "ErrorNoTemplatesSelected");
        }
        catch
        {
            return (false, [], "ErrorInvalidAction");
        }
    }

    [AllowAnonymous]
    public async Task<IActionResult> View(int id)
    {
        var userId = User.Identity?.IsAuthenticated == true ? GetUserId() : null;
        var isAdmin = User.IsInRole("Admin");
        var template = await _templateService.GetPublicTemplateAsync(id, userId, isAdmin);
        if (template == null) return NotFound();
        template.Forms = await _formService.GetFormsForTemplateAsync(id, userId, isAdmin);
        template.Comments = await _commentService.GetCommentsAsync(id);
        return View(template);
    }

    private IActionResult HandleSuccess(string messageKey)
    {
        TempData["Success"] = _localizer[messageKey].Value;
        return RedirectToAction(nameof(Index));
    }

    private IActionResult HandleError(Template? template, string errorKey)
    {
        TempData["Error"] = _localizer[errorKey].Value;
        return template != null ? View(template) : NotFound();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddQuestion(int templateId, Question question)
    {
        if (!ModelState.IsValid) return Json(new { succeeded = false, error = "InvalidModelState" });

        var userId = GetUserId();
        var isAdmin = User.IsInRole("Admin");
        var (succeeded, error) = await _questionService.AddQuestionAsync(templateId, question, userId, isAdmin);

        if (!succeeded) return Json(new { succeeded = false, error });

        var questionDto = new QuestionDto
        {
            Id = question.Id,
            TemplateId = question.TemplateId,
            Title = question.Title,
            Description = question.Description,
            Type = question.Type,
            Order = question.Order,
            IsVisibleInResults = question.IsVisibleInResults
        };

        return Json(new { succeeded = true, error = string.Empty, question = questionDto });
    }

    [HttpGet]
    public async Task<IActionResult> EditQuestion(int id)
    {
        var userId = GetUserId();
        var isAdmin = User.IsInRole("Admin");
        var question = await _questionService.GetQuestionAsync(id, userId, isAdmin);
        if (question == null)
            return NotFound();
        return PartialView("_EditQuestion", question);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditQuestion(int id, Question question)
    {
        if (!ModelState.IsValid) return View(question);
        var userId = GetUserId();
        var isAdmin = User.IsInRole("Admin");
        var (succeeded, error) = await _questionService.UpdateQuestionAsync(id, question, userId, isAdmin);
        if (!succeeded)
        {
            ModelState.AddModelError("", _localizer[error].Value);
            return View(question);
        }

        TempData["Success"] = _localizer["SuccessQuestionUpdated"].Value;
        return RedirectToAction(nameof(Edit), new { id = question.TemplateId });
    }

    [HttpGet]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        var userId = GetUserId();
        var isAdmin = User.IsInRole("Admin");
        var question = await _questionService.GetQuestionAsync(id, userId, isAdmin);
        if (question == null)
        {
            TempData["Error"] = _localizer["QuestionNotFoundOrAccessDenied"].Value;
            return RedirectToAction(nameof(Index));
        }

        var templateId = question.TemplateId;
        var (succeeded, error) = await _questionService.DeleteQuestionAsync(id, userId, isAdmin);
        TempData[succeeded ? "Success" : "Error"] = _localizer[succeeded ? "SuccessQuestionDeleted" : error].Value;
        return RedirectToAction(nameof(Edit), new { id = templateId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuestionOrder(List<QuestionOrder> orders)
    {
        var userId = GetUserId();
        var isAdmin = User.IsInRole("Admin");
        var (succeeded, error) = await _questionService.UpdateQuestionOrderAsync(orders, userId, isAdmin);
        return Json(new { succeeded, error });
    }
    
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int templateId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            TempData["Error"] = _localizer["CommentRequired"].Value;
            return RedirectToAction(nameof(View), new { id = templateId });
        }

        var userId = GetUserId();
        var (succeeded, error) = await _commentService.AddCommentAsync(templateId, content, userId);
        if (!succeeded)
        {
            TempData["Error"] = _localizer[error].Value;
            return RedirectToAction(nameof(View), new { id = templateId });
        }

        TempData["Success"] = _localizer["SuccessCommentAdded"].Value;
        return RedirectToAction(nameof(View), new { id = templateId });
    }
    
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteComment(int commentId, int templateId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            TempData["Error"] = _localizer["UserNotAuthenticated"].Value;
            return RedirectToAction(nameof(View), new { id = templateId });
        }

        var (succeeded, error) = await _commentService.DeleteCommentAsync(commentId, userId);
        if (!succeeded)
        {
            TempData["Error"] = _localizer[error].Value;
            return RedirectToAction(nameof(View), new { id = templateId });
        }

        TempData["Success"] = _localizer["CommentDeleted"].Value;
        return RedirectToAction(nameof(View), new { id = templateId });
    }
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLike(int templateId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            TempData["Error"] = _localizer["UserNotAuthenticated"].Value;
            return RedirectToAction(nameof(View), new { id = templateId });
        }

        var isLiked = await _likeService.IsLikedAsync(templateId, userId);
        if (isLiked)
        {
            var (succeeded, error) = await _likeService.RemoveLikeAsync(templateId, userId);
            if (!succeeded)
            {
                TempData["Error"] = _localizer[error].Value;
                return RedirectToAction(nameof(View), new { id = templateId });
            }
            TempData["Success"] = _localizer["UnlikeSuccess"].Value;
        }
        else
        {
            var (succeeded, error) = await _likeService.AddLikeAsync(templateId, userId);
            if (!succeeded)
            {
                TempData["Error"] = _localizer[error].Value;
                return RedirectToAction(nameof(View), new { id = templateId });
            }
            TempData["Success"] = _localizer["LikeSuccess"].Value;
        }

        return RedirectToAction(nameof(View), new { id = templateId });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> IsLiked(int templateId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Json(false);

        var isLiked = await _likeService.IsLikedAsync(templateId, userId);
        return Json(isLiked);
    }
    [HttpGet]
    public async Task<IActionResult> SearchTags(string term)
    {
        var tags = await _templateService.SearchTagsAsync(term);
        return Json(tags.Select(t => new { id = t, text = t }));
    }
}