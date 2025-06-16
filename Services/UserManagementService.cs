using CourseProject.Models;
using CourseProject.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CourseProject.Services;

public class UserManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserManagementService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    
    public async Task<List<UserViewModel>> GetAllUsersViewModelAsync()
    {
        var users = await _userManager.Users.AsNoTracking().ToListAsync();
        var userViewModels = new List<UserViewModel>();

        foreach (var user in users)
        {
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            userViewModels.Add(new UserViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                IsLocked = user.LockoutEnd != null,
                IsAdmin = isAdmin
            });
        }

        return userViewModels;
    }

    public async Task<bool> BlockUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        user.LockoutEnd = DateTimeOffset.MaxValue;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> UnblockUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        user.LockoutEnd = null;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> ToggleAdminRoleAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            var result = await _userManager.RemoveFromRoleAsync(user, "Admin");
            return result.Succeeded;
        }
        else
        {
            var result = await _userManager.AddToRoleAsync(user, "Admin");
            return result.Succeeded;
        }
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }
}