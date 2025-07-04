using CourseProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourseProject.Controllers;

public class TemplateAccessController : Controller
{
    private readonly TemplateAccessService _templateAccessService;
    
    public TemplateAccessController(TemplateAccessService templateAccessService)
    {
        _templateAccessService = templateAccessService;
    }

    [HttpPost]
    public async Task<IActionResult> AddUserAccess(int templateId, string userId)
    {
        try
        {
            await _templateAccessService.AddUserAccessAsync(templateId, userId);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> RemoveUserAccess(int templateId, string userId)
    {
        await _templateAccessService.RemoveUserAccessAsync(templateId, userId);
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> SearchUsers(string query)
    {
        var users = await _templateAccessService.SearchUsersAsync(query);
        return Json(users.Select(u => new { u.Id, u.Name, u.Email }));
    }
} 