using CourseProject.Models;
using CourseProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CourseProject.Controllers;

[Authorize]
public class TemplateController : Controller
{
    private readonly ITemplateService _templateService;

    public TemplateController(ITemplateService templateService)
    {
        _templateService = templateService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var templates = await _templateService.GetTemplatesAsync(User.Identity.IsAuthenticated, userId);
        return View(templates);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new TemplateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TemplateViewModel model, IFormFile? Image)
    {
        if (!ModelState.IsValid) return View(model);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _templateService.CreateTemplateAsync(model, userId, Image);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var template = await _templateService.GetTemplateAsync(id, User.Identity.IsAuthenticated, userId);
        return template == null ? NotFound() : View(template);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!await _templateService.IsOwnerOrAdminAsync(id, userId)) return Unauthorized();
        var template = await _templateService.GetTemplateAsync(id, true, userId);
        return template == null ? NotFound() : View(template);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TemplateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            await _templateService.UpdateTemplateAsync(model, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return RedirectToAction(nameof(Index));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _templateService.DeleteTemplateAsync(id, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return RedirectToAction(nameof(Index));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetTags(string term)
    {
        var tags = await _templateService.GetTagsAsync(term);
        return Json(tags);
    }
}