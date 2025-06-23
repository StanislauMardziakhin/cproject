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
    private readonly TemplateService _templateService;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public TemplatesController(TemplateService templateService, IStringLocalizer<SharedResources> localizer)
    {
        _templateService = templateService;
        _localizer = localizer;
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
        if (template == null)
        {
            return NotFound();
        }
        return View(template);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Template template, IFormFile? image)
    {
        if (id != template.Id || !ModelState.IsValid) return HandleError(template, "TErrorInvalidInput");
        {
            bool isAdmin = User.IsInRole("Admin");
            var (succeeded, error) = await _templateService.UpdateAsync(id, template, image, GetUserId(), isAdmin);
            return succeeded ? HandleSuccess("SuccessTemplateUpdated") : HandleError(template, error);
        }
        
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
        if (string.IsNullOrEmpty(templateIds) || !Enum.TryParse<TemplateActionType>(action, true, out var actionType))
        {
            TempData["Error"] = _localizer["ErrorInvalidAction"].Value;
            return RedirectToAction(nameof(Index));
        }
        
        var ids = templateIds.Split(',').Select(int.Parse).ToArray();
        if (!ids.Any())
        {
            TempData["Error"] = _localizer["AdminErrorNoUsersSelected"].Value;
            return RedirectToAction(nameof(Index));
        }

        var userId = GetUserId();
        var isAdmin = User.IsInRole("Admin");
        var (succeeded, error) = await _templateService.ApplyMassActionAsync(action.ToLower(), ids, userId, isAdmin);
        return succeeded ? HandleSuccess($"Success{action}Template") : HandleError(null, error);
    }
    
    [AllowAnonymous]
    public async Task<IActionResult> View(int id)
    {
        var userId = User.Identity?.IsAuthenticated == true ? GetUserId() : null;
        var isAdmin = User.IsInRole("Admin");
        var template = await _templateService.GetPublicTemplateAsync(id, userId, isAdmin);
        if (template == null)
        {
            return NotFound();
        }
        return View(template);
    }
    
    private string GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
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
}