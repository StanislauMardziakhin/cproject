using CourseProject.Models;
using CourseProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using CourseProject.ViewModels;
using System.Security.Claims;

namespace CourseProject.Controllers;

[Authorize(Roles = "Admin")]
public class AdminFormsController : Controller
{
    private readonly FormService _formService;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public AdminFormsController(FormService formService, IStringLocalizer<SharedResources> localizer)
    {
        _formService = formService;
        _localizer = localizer;
    }

    private string GetUserId() => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    private bool IsAdmin() => User.IsInRole("Admin");

    private async Task<Form?> ReloadFormForEditing(int formId)
    {
        return await _formService.GetFormForEditingAsync(formId, GetUserId(), IsAdmin());
    }

    private IActionResult HandleFormError(string errorKey)
    {
        TempData["Error"] = _localizer[errorKey].Value;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var form = await ReloadFormForEditing(id);
        if (form == null) return NotFound();
        return View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Form form, Dictionary<int, string> answers)
    {
        if (!ModelState.IsValid)
        {
            var reloadedForm = await ReloadFormForEditing(form.Id);
            if (reloadedForm == null) return NotFound();
            return View(reloadedForm);
        }
        var (success, errorKey) = await _formService.EditFormAsync(form.Id, answers, GetUserId(), IsAdmin());
        if (!success)
        {
            ModelState.AddModelError(string.Empty, _localizer[errorKey]);
            var reloadedForm = await ReloadFormForEditing(form.Id);
            if (reloadedForm == null) return NotFound();
            return View(reloadedForm);
        }
        TempData["Success"] = _localizer["SuccessFormUpdated"].Value;
        return RedirectToAction("View", "UserForms", new { id = form.Id });
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

    private FormResultViewModel ToFormResultViewModel(Form form)
    {
        return new FormResultViewModel
        {
            FormId = form.Id,
            UserEmail = form.User?.Email,
            CreatedAt = form.CreatedAt,
            Answers = form.Answers.Select(a => new FormAnswerViewModel
            {
                QuestionTitle = a.Question.Title,
                Value = a.Value
            }).ToList()
        };
    }

    public async Task<IActionResult> View(int id, string? returnUrl)
    {
        var userId = User?.Identity?.IsAuthenticated == true ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;
        var isAdmin = IsAdmin();
        var form = await _formService.GetFormForViewingAsync(id, userId, isAdmin);
        if (form == null) return NotFound();
        var viewModel = ToFormResultViewModel(form);
        return View(viewModel);
    }
} 