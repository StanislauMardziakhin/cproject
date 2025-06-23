using System.Security.Claims;
using CourseProject.Models;
using CourseProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CourseProject.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private static readonly Dictionary<UserActionType, Func<UserManagementService, string[], string?, Task<bool>>>
        _actions = new()
        {
            [UserActionType.Block] = async (service, ids, _) =>
            {
                await service.BlockUserAsync(ids);
                return false;
            },
            [UserActionType.Unblock] = async (service, ids, _) =>
            {
                await service.UnblockUserAsync(ids);
                return false;
            },
            [UserActionType.AssignAdmin] = async (service, ids, _) =>
            {
                await service.AssignAdminRoleAsync(ids);
                return false;
            },
            [UserActionType.RemoveAdmin] =
                (service, ids, currentUserId) => service.RemoveAdminRoleAsync(ids, currentUserId),
            [UserActionType.Delete] = (service, ids, currentUserId) => service.DeleteUsersAsync(ids, currentUserId)
        };

    private readonly UserManagementService _userManagementService;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public AdminController(UserManagementService userManagementService, IStringLocalizer<SharedResources> localizer)
    {
        _userManagementService = userManagementService;
        _localizer = localizer;
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
        if (userIds == null || !userIds.Any() || !Enum.TryParse<UserActionType>(action, true, out var actionType))
        {
            TempData["Error"] = _localizer["ErrorInvalidAction"].Value;
            return RedirectToAction("Index");
        }
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var requiresLogout = await _actions[actionType](_userManagementService, userIds, currentUserId);
        TempData["Success"] = string.Format(_localizer["SuccessActionApplied"].Value, actionType);
        return requiresLogout
            ? RedirectToAction("Index", "Home")
            : RedirectToAction(nameof(Index));
    }
}