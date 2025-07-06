using System.Security.Claims;
using CourseProject.Models;
using CourseProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using CourseProject.ViewModels;

namespace CourseProject.Controllers;

[Authorize]
public class FormsController : Controller
{
    private readonly FormService _formService;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public FormsController(FormService formService, IStringLocalizer<SharedResources> localizer)
    {
        _formService = formService;
        _localizer = localizer;
    }

    private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    private bool IsAdmin() => User.IsInRole("Admin");

    private async Task<Form?> ReloadFormForFilling(int templateId)
    {
        return await _formService.GetFormForFillingAsync(templateId, GetUserId(), IsAdmin());
    }

    private async Task<Form?> ReloadFormForEditing(int formId)
    {
        return await _formService.GetFormForEditingAsync(formId, GetUserId(), IsAdmin());
    }

    private IActionResult HandleFormError(string errorKey)
    {
        TempData["Error"] = _localizer[errorKey].Value;
        return RedirectToAction(nameof(Index));
    }

    private FormResultViewModel ToFormResultViewModel(Form form)
    {
        return new FormResultViewModel
        {
            FormId = form.Id,
            TemplateId = form.TemplateId,
            TemplateName = form.Template?.Name,
            UserEmail = form.User?.Email,
            UserName = form.User?.Name,
            UserId = form.UserId,
            CreatedAt = form.CreatedAt,
            Answers = form.Answers.Select(a => new FormAnswerViewModel
            {
                QuestionId = a.QuestionId,
                QuestionTitle = a.Question.Title,
                IsVisibleInResults = a.Question.IsVisibleInResults,
                QuestionType = a.Question.Type,
                Value = a.Value
            }).ToList()
        };
    }

    [HttpGet]
    public async Task<IActionResult> Fill(int templateId)
    {
        var form = await ReloadFormForFilling(templateId);
        if (form == null) return NotFound();
        var viewModel = ToFormResultViewModel(form);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Fill(int templateId, Dictionary<int, string> answers)
    {
        var form = new Form { TemplateId = templateId };
        var (success, errorMessage) = await _formService.SaveFormAsync(form, answers, GetUserId());
        if (!success)
        {
            ModelState.AddModelError(string.Empty, _localizer[errorMessage].Value);
            var reloadedForm = await ReloadFormForFilling(templateId);
            if (reloadedForm == null) return NotFound();
            var viewModel = ToFormResultViewModel(reloadedForm);
            return View(viewModel);
        }
        TempData["Success"] = _localizer["SuccessFormSubmitted"].Value;
        return RedirectToAction("Index", "Templates");
    }

    [HttpGet]
    public async Task<IActionResult> View(int id, string? returnUrl)
    {
        var userId = GetUserId();
        var isAdmin = IsAdmin();
        var form = await _formService.GetFormForViewingAsync(id, userId, isAdmin);
        if (form == null) return NotFound();
        var viewModel = ToFormResultViewModel(form);
        ViewData["BackUrl"] = !string.IsNullOrEmpty(returnUrl)
            ? returnUrl
            : isAdmin || form.Template?.UserId == userId
                ? Url.Action("Edit", "Templates", new { id = form.TemplateId }) + "#results"
                : Url.Action("Index", "Forms");
        return View(viewModel);
    }

    public async Task<IActionResult> Index()
    {
        var forms = await _formService.GetFormsForUserAsync(GetUserId());
        var viewModels = forms.Select(ToFormResultViewModel).ToList();
        return View(viewModels);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyAction(string action, string formIds)
    {
        if (string.IsNullOrWhiteSpace(formIds))
            return HandleFormError("ErrorNoFormsSelected");
        if (!string.Equals(action, "Delete", StringComparison.OrdinalIgnoreCase))
            return HandleFormError("ErrorInvalidAction");
        var ids = formIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(id => int.TryParse(id, out var parsedId) ? parsedId : (int?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();
        if (!ids.Any())
            return HandleFormError("ErrorNoValidForms");
        var (success, errorKey) = await _formService.DeleteFormsAsync(ids, GetUserId(), IsAdmin());
        if (!success)
            return HandleFormError(errorKey);
        TempData["Success"] = _localizer["SuccessDeleteForms"].Value;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var form = await ReloadFormForEditing(id);
        if (form == null) return NotFound();
        var viewModel = ToFormResultViewModel(form);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Form form, Dictionary<int, string> answers)
    {
        if (!ModelState.IsValid)
        {
            var reloadedForm = await ReloadFormForEditing(form.Id);
            if (reloadedForm == null) return NotFound();
            var viewModel = ToFormResultViewModel(reloadedForm);
            return View(viewModel);
        }
        var (success, errorKey) = await _formService.EditFormAsync(form.Id, answers, GetUserId(), IsAdmin());
        if (!success)
        {
            ModelState.AddModelError(string.Empty, _localizer[errorKey]);
            var reloadedForm = await ReloadFormForEditing(form.Id);
            if (reloadedForm == null) return NotFound();
            var viewModel = ToFormResultViewModel(reloadedForm);
            return View(viewModel);
        }
        TempData["Success"] = _localizer["SuccessFormUpdated"].Value;
        return RedirectToAction("View", new { id = form.Id });
    }
}