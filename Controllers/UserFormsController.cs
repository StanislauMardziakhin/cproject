using CourseProject.Models;
using CourseProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using CourseProject.ViewModels;

namespace CourseProject.Controllers;

[Authorize]
public class UserFormsController : Controller
{
    private readonly FormService _formService;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public UserFormsController(FormService formService, IStringLocalizer<SharedResources> localizer)
    {
        _formService = formService;
        _localizer = localizer;
    }

    private string GetUserId() => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    private bool IsAdmin() => User.IsInRole("Admin");

    private async Task<Form?> ReloadFormForFilling(int templateId)
    {
        return await _formService.GetFormForFillingAsync(templateId, GetUserId(), IsAdmin());
    }

    [HttpGet]
    public async Task<IActionResult> Fill(int templateId)
    {
        var form = await ReloadFormForFilling(templateId);
        if (form == null) return NotFound();
        return View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Fill(Form form, Dictionary<int, string> answers)
    {
        if (!ModelState.IsValid)
        {
            var reloadedForm = await ReloadFormForFilling(form.TemplateId);
            if (reloadedForm == null) return NotFound();
            return View(reloadedForm);
        }
        var (success, errorMessage) = await _formService.SaveFormAsync(form, answers, GetUserId());
        if (!success)
        {
            ModelState.AddModelError(string.Empty, _localizer[errorMessage].Value);
            var reloadedForm = await ReloadFormForFilling(form.TemplateId);
            if (reloadedForm == null) return NotFound();
            return View(reloadedForm);
        }
        TempData["Success"] = _localizer["SuccessFormSubmitted"].Value;
        return RedirectToAction("Index", "Templates");
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
            : isAdmin || form.Template.UserId == userId
                ? Url.Action("Edit", "Templates", new { id = form.TemplateId }) + "#results"
                : Url.Action("Index", "UserForms");
        return View(viewModel);
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var forms = await _formService.GetFormsForUserAsync(GetUserId());
        return View(forms);
    }
} 