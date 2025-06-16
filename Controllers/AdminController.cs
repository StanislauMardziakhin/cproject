using CourseProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseProject.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManagementService _userManagementService;

    public AdminController(UserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }
    
    public async Task<IActionResult> Index()
    {
        var users = await _userManagementService.GetAllUsersViewModelAsync();
        return View(users);
    }
    
    [HttpPost]
    public async Task<IActionResult> Block(string id)
    {
        await _userManagementService.BlockUserAsync(id);
        return RedirectToAction("Index");
    }
    
    [HttpPost]
    public async Task<IActionResult> Unblock(string id)
    {
        await _userManagementService.UnblockUserAsync(id);
        return RedirectToAction("Index");
    }
    
    [HttpPost]
    public async Task<IActionResult> ToggleAdmin(string id)
    {
        await _userManagementService.ToggleAdminRoleAsync(id);
        return RedirectToAction("Index");
    }
    
    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        await _userManagementService.DeleteUserAsync(id); 
        return RedirectToAction("Index");
    }
}