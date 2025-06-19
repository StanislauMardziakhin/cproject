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
    
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        var templates = await _templateService.GetUserTemplatesAsync(userId);
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
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            Console.WriteLine("ModelState errors: " + string.Join(", ", errors));
            return HandleError(template, "TErrorInvalidInput");
        }
        var (succeeded, error) = await _templateService.CreateAsync(template, image, GetUserId());
        return succeeded ? HandleSuccess("SuccessTemplateCreated") : HandleError(template, error);
    }
    
    public async Task<IActionResult> Edit(int id)
    {
        var template = await _templateService.GetForEditAsync(id, GetUserId());
        return template != null ? View(template) : NotFound();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Template template, IFormFile? image)
    {
        if (id != template.Id || !ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            Console.WriteLine("ModelState errors: " + string.Join(", ", errors));
            return HandleError(template, "TErrorInvalidInput");
        }
        var (succeeded, error) = await _templateService.UpdateAsync(id, template, image, GetUserId());
        return succeeded ? HandleSuccess("SuccessTemplateUpdated") : HandleError(template, error);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (succeeded, error) = await _templateService.DeleteAsync(id, GetUserId());
        return succeeded ? HandleSuccess("SuccessTemplateDeleted") : HandleError(null, error);
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