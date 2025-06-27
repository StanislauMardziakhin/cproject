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

    public TemplatesController(TemplateService templateService, IStringLocalizer<SharedResources> localizer,
        QuestionService questionService)
    {
        _templateService = templateService;
        _localizer = localizer;
        _questionService = questionService;
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
        var (actionSucceeded, error) =
            await _templateService.ApplyMassActionAsync(action.ToLower(), ids, userId, isAdmin);
        return actionSucceeded ? HandleSuccess($"Success{action}Template") : HandleError(null, error);
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
}