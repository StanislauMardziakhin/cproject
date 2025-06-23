using CourseProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseProject.Controllers;

[AllowAnonymous]
public class SearchController : Controller
{
    private readonly TemplateService _templateService;

    public SearchController(TemplateService templateService)
    {
        _templateService = templateService;
    }

    public async Task<IActionResult> Index(string query)
    {
        var templates = await _templateService.SearchAsync(query);
        return View(templates);
    }
}