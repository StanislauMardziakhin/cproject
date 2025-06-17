using System.Security.Claims;
using CourseProject.Models;
using CourseProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseProject.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private static readonly Dictionary<ActionType, Func<UserManagementService, string[], string?, Task<bool>>>
        _actions = new()
        {
            [ActionType.Block] = async (service, ids, _) =>
            {
                await service.BlockUserAsync(ids);
                return false;
            },
            [ActionType.Unblock] = async (service, ids, _) =>
            {
                await service.UnblockUserAsync(ids);
                return false;
            },
            [ActionType.AssignAdmin] = async (service, ids, _) =>
            {
                await service.AssignAdminRoleAsync(ids);
                return false;
            },
            [ActionType.RemoveAdmin] =
                (service, ids, currentUserId) => service.RemoveAdminRoleAsync(ids, currentUserId),
            [ActionType.Delete] = (service, ids, currentUserId) => service.DeleteUsersAsync(ids, currentUserId)
        };

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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyAction(string[] userIds, string action)
    {
        if (userIds == null || !userIds.Any() || !Enum.TryParse<ActionType>(action, true, out var actionType))
            return RedirectToAction("Index");
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var requiresLogout = await _actions[actionType](_userManagementService, userIds, currentUserId);

        return requiresLogout
            ? RedirectToAction("Index", "Home")
            : RedirectToAction(nameof(Index));
    }
}