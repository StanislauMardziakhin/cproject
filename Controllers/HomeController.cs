using System.Diagnostics;
using CourseProject.Models;
using CourseProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CourseProject.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TemplateService _templateService;

    public HomeController(AppDbContext context, UserManager<ApplicationUser> userManager, TemplateService templateService)
    {
        _context = context;
        _userManager = userManager;
        _templateService = templateService;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var userId = user?.Id;
        var isAdmin = User.IsInRole("Admin");

        ViewData["UserName"] = user?.Name ?? "Guest";
        var latestTemplates = await _templateService.GetLatestTemplatesAsync(userId, isAdmin);
        var topTemplates = await _templateService.GetTopTemplatesByFormsAsync(userId, isAdmin);
        ViewData["LatestTemplates"] = latestTemplates;
        ViewData["TopTemplates"] = topTemplates;
        return View();
    }

    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}