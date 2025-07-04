using System.Security.Claims;
using CourseProject.Models;
using CourseProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using CourseProject.ViewModels;

namespace CourseProject.Controllers;

[Authorize]
public class TemplatesController : Controller
{
    private readonly IStringLocalizer<SharedResources> _localizer;
    private readonly TemplateService _templateService;
    private readonly FormService _formService;
    private readonly IFormResultService _formResultService;
    private readonly IResultsAggregatorService _resultsAggregatorService;
    private readonly CommentService _commentService;
    private readonly LikeService _likeService;
    private readonly TemplateAccessService _templateAccessService;
    private readonly TemplatePublicationService _templatePublicationService;

    public TemplatesController(
        TemplateService templateService,
        IStringLocalizer<SharedResources> localizer,
        FormService formService,
        IFormResultService formResultService,
        IResultsAggregatorService resultsAggregatorService,
        CommentService commentService,
        LikeService likeService,
        TemplateAccessService templateAccessService,
        TemplatePublicationService templatePublicationService)
    {
        _templateService = templateService;
        _localizer = localizer;
        _formService = formService;
        _formResultService = formResultService;
        _resultsAggregatorService = resultsAggregatorService;
        _commentService = commentService;
        _likeService = likeService;
        _templateAccessService = templateAccessService;
        _templatePublicationService = templatePublicationService;
    }

    private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    private bool IsAdmin() => User.IsInRole("Admin");
    private string? GetUserIdIfAuthenticated() => User?.Identity?.IsAuthenticated == true ? GetUserId() : null;

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

    private IActionResult RedirectToViewWithMessage(int templateId, string messageKey, bool success)
    {
        TempData[success ? "Success" : "Error"] = _localizer[messageKey].Value;
        return RedirectToAction(nameof(View), new { id = templateId });
    }
    
    private TemplateViewModel ToViewModel(Template template, string? currentUserId = null)
    {
        return new TemplateViewModel
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            Theme = template.Theme,
            IsPublic = template.IsPublic,
            Tags = template.Tags,
            ImageUrl = template.ImageUrl,
            LikeCount = template.Likes?.Count ?? 0,
            CommentCount = template.Comments?.Count ?? 0,
            IsLikedByCurrentUser = currentUserId != null && template.Likes?.Any(l => l.UserId == currentUserId) == true,
            OwnerName = template.User?.Name,
            UserId = template.UserId,
            TemplateAccesses = template.TemplateAccesses?.Select(a => new TemplateAccessViewModel
            {
                UserId = a.UserId,
                UserName = a.User?.Name ?? string.Empty,
                UserEmail = a.User?.Email ?? string.Empty
            }).ToList() ?? new(),
            Forms = template.Forms?.Select(f => new FormResultViewModel
            {
                FormId = f.Id,
                TemplateId = f.TemplateId,
                TemplateName = template.Name,
                UserEmail = f.User?.Email,
                UserName = f.User?.Name,
                UserId = f.UserId,
                CreatedAt = f.CreatedAt,
                Answers = f.Answers.Select(ans => new FormAnswerViewModel
                {
                    QuestionId = ans.QuestionId,
                    QuestionTitle = ans.Question.Title,
                    IsVisibleInResults = ans.Question.IsVisibleInResults,
                    Value = ans.Value
                }).ToList()
            }).ToList() ?? new(),
            QuestionsViewModel = template.Questions?.Select(q => new QuestionViewModel
            {
                Id = q.Id,
                TemplateId = q.TemplateId,
                Title = q.Title,
                Description = q.Description,
                Type = q.Type.ToString(),
                Order = q.Order,
                IsVisibleInResults = q.IsVisibleInResults
            }).ToList() ?? new(),
            Likes = template.Likes?.Select(l => new LikeViewModel
            {
                UserId = l.UserId
            }).ToList() ?? new(),
            Comments = template.Comments?.Select(c => new CommentViewModel
            {
                Id = c.Id,
                Content = c.Content,
                UserName = c.User?.Name ?? string.Empty,
                CreatedAt = c.CreatedAt
            }).ToList() ?? new()
        };
    }

    private void UpdateTemplateFromViewModel(Template template, TemplateViewModel model)
    {
        template.Name = model.Name;
        template.Description = model.Description;
        template.Theme = model.Theme;
        template.IsPublic = model.IsPublic;
        template.Tags = model.Tags;
        template.ImageUrl = model.ImageUrl;
    }

    public async Task<IActionResult> Index(bool? publicOnly = null)
    {
        var templates = await _templateService.GetUserTemplatesAsync(GetUserIdIfAuthenticated(), publicOnly);
        var currentUserId = GetUserIdIfAuthenticated();
        var viewModels = templates.Select(t => ToViewModel(t, currentUserId)).ToList();
        return View(viewModels);
    }

    public IActionResult Create() => View(new TemplateViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TemplateViewModel model, IFormFile? image)
    {
        if (!ModelState.IsValid) return View(model);
        var template = new Template();
        UpdateTemplateFromViewModel(template, model);
        template.UserId = GetUserId();
        var (succeeded, error) = await _templateService.CreateAsync(template, image, template.UserId);
        if (succeeded) return HandleSuccess("SuccessTemplateCreated");
        ModelState.AddModelError("", _localizer[error].Value);
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var userId = GetUserIdIfAuthenticated();
        var isAdmin = IsAdmin();
        var template = await _templateService.GetForEditAsync(id, userId, isAdmin);
        if (template == null) return NotFound();
        template.Forms = await _formService.GetFormsForTemplateAsync(id, userId, isAdmin);
        ViewData["AggregatedResults"] = await _resultsAggregatorService.GetAggregatedResultsAsync(id);
        var model = ToViewModel(template, userId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TemplateViewModel model, IFormFile? image)
    {
        if (id != model.Id || !ModelState.IsValid) return View(model);
        var template = await _templateService.GetForEditAsync(id, GetUserIdIfAuthenticated(), IsAdmin());
        if (template == null) return NotFound();
        UpdateTemplateFromViewModel(template, model);
        var (succeeded, error) = await _templateService.UpdateAsync(id, template, image, GetUserId(), IsAdmin());
        if (succeeded) return HandleSuccess("SuccessTemplateUpdated");
        ModelState.AddModelError("", _localizer[error].Value);
        return View(model);
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
        var (actionSucceeded, error) = await _templatePublicationService.ApplyMassActionAsync(action.ToLower(), ids, GetUserId(), IsAdmin());
        return actionSucceeded ? HandleSuccess(GetSuccessMessageKey(action)) : HandleError(null, error);
    }

    [AllowAnonymous]
    public async Task<IActionResult> View(int id)
    {
        var userId = GetUserIdIfAuthenticated();
        var isAdmin = IsAdmin();
        var template = await _templateService.GetPublicTemplateAsync(id, userId, isAdmin);
        if (template == null) return NotFound();
        template.Forms = await _formService.GetFormsForTemplateAsync(id, userId, isAdmin);
        template.Comments = await _commentService.GetCommentsAsync(id);
        var model = ToViewModel(template, userId);
        return View(model);
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

    private string GetSuccessMessageKey(string action)
    {
        return action.ToLower() switch
        {
            "delete" => "SuccessDeleteTemplate",
            "publish" => "SuccessPublishTemplate",
            "unpublish" => "SuccessUnpublishTemplate",
            _ => "ErrorInvalidAction"
        };
    }

    [HttpGet]
    public async Task<IActionResult> SearchTags(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return Json(new List<string>());
        
        var tags = await _templateService.SearchTagsAsync(term);
        return Json(tags);
    }
}