using System.Security.Claims;
using CourseProject.Models;
using CourseProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

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

    private string GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }

    [HttpGet]
    public async Task<IActionResult> Fill(int templateId)
    {
        var form = await _formService.GetFormForFillingAsync(templateId, GetUserId(), User.IsInRole("Admin"));
        if (form == null) return NotFound();
        return View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Fill(Form form, Dictionary<int, string> answers)
    {
        if (!ModelState.IsValid)
        {
            var reloadedForm =
                await _formService.GetFormForFillingAsync(form.TemplateId, GetUserId(), User.IsInRole("Admin"));
            if (reloadedForm == null) return NotFound();
            return View(reloadedForm);
        }

        var (success, errorMessage) = await _formService.SaveFormAsync(form, answers, GetUserId());
        if (!success)
        {
            ModelState.AddModelError(string.Empty, _localizer[errorMessage].Value);
            var reloadedForm =
                await _formService.GetFormForFillingAsync(form.TemplateId, GetUserId(), User.IsInRole("Admin"));
            if (reloadedForm == null) return NotFound();
            return View(reloadedForm);
        }

        TempData["Success"] = _localizer["SuccessFormSubmitted"].Value;
        return RedirectToAction("Index", "Templates");
    }

    [HttpGet]
    public async Task<IActionResult> View(int id, string? returnUrl)
    {
        var userId = GetUserId();
        var isAdmin = User.IsInRole("Admin");
        var form = await _formService.GetFormForViewingAsync(id, userId, isAdmin);
        if (form == null) return NotFound();
        ViewData["BackUrl"] = !string.IsNullOrEmpty(returnUrl)
            ? returnUrl
            : isAdmin || form.Template.UserId == userId
                ? Url.Action("Edit", "Templates", new { id = form.TemplateId }) + "#results"
                : Url.Action("Index", "Forms");
        return View(form);
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var forms = await _formService.GetFormsForUserAsync(userId);
        return View(forms);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyAction(string action, string formIds)
    {
        if (string.IsNullOrWhiteSpace(formIds))
        {
            TempData["Error"] = _localizer["ErrorNoFormsSelected"].Value;
            return RedirectToAction(nameof(Index));
        }

        if (!string.Equals(action, "Delete", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = _localizer["ErrorInvalidAction"].Value;
            return RedirectToAction(nameof(Index));
        }

        var ids = formIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(id => int.TryParse(id, out var parsedId) ? parsedId : (int?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();

        if (!ids.Any())
        {
            TempData["Error"] = _localizer["ErrorNoValidForms"].Value;
            return RedirectToAction(nameof(Index));
        }

        var userId = GetUserId();
        var isAdmin = User.IsInRole("Admin");

        var (success, errorKey) = await _formService.DeleteFormsAsync(ids, userId, isAdmin);

        if (!success)
        {
            TempData["Error"] = _localizer[errorKey].Value;
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = _localizer["SuccessDeleteForms"].Value;
        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var form = await _formService.GetFormForEditingAsync(id, GetUserId(), User.IsInRole("Admin"));
        if (form == null) return NotFound();
        return View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Form form, Dictionary<int, string> answers)
    {
        if (!ModelState.IsValid)
        {
            var reloadedForm = await _formService.GetFormForEditingAsync(form.Id, GetUserId(), User.IsInRole("Admin"));
            if (reloadedForm == null) return NotFound();
            return View(reloadedForm);
        }

        var (success, errorKey) = await _formService.EditFormAsync(form.Id, answers, GetUserId(), User.IsInRole("Admin"));
        if (!success)
        {
            ModelState.AddModelError(string.Empty, _localizer[errorKey]);
            var reloadedForm = await _formService.GetFormForEditingAsync(form.Id, GetUserId(), User.IsInRole("Admin"));
            if (reloadedForm == null) return NotFound();
            return View(reloadedForm);
        }

        TempData["Success"] = _localizer["SuccessFormUpdated"].Value;
        return RedirectToAction("View", new { id = form.Id });
    }
}