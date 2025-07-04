using CourseProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CourseProject.Controllers;

public class TemplatePublicationController : Controller
{
    private readonly TemplatePublicationService _templatePublicationService;
    private readonly IStringLocalizer<SharedResources> _localizer;
    
    public TemplatePublicationController(TemplatePublicationService templatePublicationService, IStringLocalizer<SharedResources> localizer)
    {
        _templatePublicationService = templatePublicationService;
        _localizer = localizer;
    }

    private string GetUserId() => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    private bool IsAdmin() => User.IsInRole("Admin");

    private IActionResult HandleSuccess(string messageKey)
    {
        TempData["Success"] = _localizer[messageKey].Value;
        return RedirectToAction("Index", "Templates");
    }

    private IActionResult HandleError(string errorKey)
    {
        TempData["Error"] = _localizer[errorKey].Value;
        return RedirectToAction("Index", "Templates");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(int id)
    {
        var (succeeded, error) = await _templatePublicationService.UpdatePublicStatusAsync(id, true, GetUserId(), IsAdmin());
        return succeeded ? HandleSuccess("SuccessPublishTemplate") : HandleError(error);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unpublish(int id)
    {
        var (succeeded, error) = await _templatePublicationService.UpdatePublicStatusAsync(id, false, GetUserId(), IsAdmin());
        return succeeded ? HandleSuccess("SuccessUnpublishTemplate") : HandleError(error);
    }
}