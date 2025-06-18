using CourseProject.Models;
using CourseProject.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CourseProject.Services;

public class UserManagementService
{
    private readonly AppDbContext _dbContext;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserManagementService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
        AppDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _signInManager = signInManager;
    }

    public async Task<List<UserViewModel>> GetAllUsersViewModelAsync()
    {
        var adminRoleId = await GetAdminRoleIdAsync();
        var users = await GetUsersWithRolesAsync(adminRoleId);
        return users;
    }
    private async Task<string> GetAdminRoleIdAsync()
    {
        return await _dbContext.Roles
            .Where(r => r.Name == "Admin")
            .Select(r => r.Id)
            .FirstAsync();
    }
    private async Task<List<UserViewModel>> GetUsersWithRolesAsync(string adminRoleId)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .GroupJoin(_dbContext.UserRoles.Where(ur => ur.RoleId == adminRoleId),
                u => u.Id, ur => ur.UserId,
                (u, roles) => new UserViewModel
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    IsLocked = u.LockoutEnd != null,
                    IsAdmin = roles.Any()
                })
            .ToListAsync();
    }

    public async Task BlockUserAsync(string[] userIds)
    {
        await ExecuteInTransactionAsync(async () =>
            await FilterUsersByIds(userIds)
                .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.LockoutEnd, DateTimeOffset.MaxValue)));
    }

    public async Task UnblockUserAsync(string[] userIds)
    {
        await ExecuteInTransactionAsync(async () =>
            await FilterUsersByIds(userIds)
                .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.LockoutEnd, (DateTimeOffset?)null)));
    }

    public async Task AssignAdminRoleAsync(string[] userIds)
    {
        await ExecuteInTransactionAsync(async () =>
        {
            var adminRole = await _dbContext.Roles.FirstAsync(r => r.Name == "Admin");
            var existingUserRoles = await FilterUserRolesByIds(userIds, adminRole.Id)
                .Select(ur => ur.UserId).ToListAsync();
            var newUserRoles = userIds.Except(existingUserRoles)
                .Select(userId => new IdentityUserRole<string> { UserId = userId, RoleId = adminRole.Id });
            await _dbContext.UserRoles.AddRangeAsync(newUserRoles);
            await _dbContext.SaveChangesAsync();
        });
    }

    public async Task<bool> RemoveAdminRoleAsync(string[] userIds, string? currentUserId)
    {
        var requiresLogout = currentUserId != null && userIds.Contains(currentUserId);

        await ExecuteInTransactionAsync(async () =>
        {
            var adminRoleId = await _dbContext.Roles
                .Where(r => r.Name == "Admin")
                .Select(r => r.Id)
                .FirstAsync();
            await FilterUserRolesByIds(userIds, adminRoleId).ExecuteDeleteAsync();
        });

        if (requiresLogout) await _signInManager.SignOutAsync();

        return requiresLogout;
    }

    public async Task<bool> DeleteUsersAsync(string[] userIds, string? currentUserId)
    {
        var requiresLogout = currentUserId != null && userIds.Contains(currentUserId);

        await ExecuteInTransactionAsync(async () =>
            await FilterUsersByIds(userIds).ExecuteDeleteAsync());

        if (requiresLogout) await _signInManager.SignOutAsync();

        return requiresLogout;
    }

    private async Task ExecuteInTransactionAsync(Func<Task> operation)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        await operation();
        await transaction.CommitAsync();
    }

    private IQueryable<ApplicationUser> FilterUsersByIds(string[] userIds)
    {
        return _dbContext.Users.Where(u => userIds.Contains(u.Id));
    }

    private IQueryable<IdentityUserRole<string>>
        FilterUserRolesByIds(string[] userIds, string roleId)
    {
        return _dbContext.UserRoles.Where(ur => userIds.Contains(ur.UserId) && ur.RoleId == roleId);
    }
}