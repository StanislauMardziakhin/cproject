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

        var latestTemplates = await _templateService.GetLatestTemplatesAsync(userId, isAdmin);
        var topTemplates = await _templateService.GetTopTemplatesByFormsAsync(userId, isAdmin);

        var latestViewModels = latestTemplates.Select(t => ToViewModel(t)).ToList();
        var topViewModels = topTemplates.Select(t => ToViewModel(t)).ToList();

        var model = new ViewModels.HomePageViewModel
        {
            LatestTemplates = latestViewModels,
            TopTemplates = topViewModels
        };
        return View(model);
    }
    
    private ViewModels.TemplateViewModel ToViewModel(Template template)
    {
        return new ViewModels.TemplateViewModel
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            DescriptionTruncated = string.IsNullOrEmpty(template.Description)
                ? ""
                : template.Description.Length > 100 ? template.Description.Substring(0, 100) + "..." : template.Description,
            Theme = template.Theme,
            IsPublic = template.IsPublic,
            Tags = template.Tags,
            ImageUrl = template.ImageUrl,
            LikeCount = template.Likes?.Count ?? 0,
            CommentCount = template.Comments?.Count ?? 0,
            FormsCount = template.Forms?.Count ?? 0,
            OwnerName = template.User?.Name,
            UserId = template.UserId,
        };
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