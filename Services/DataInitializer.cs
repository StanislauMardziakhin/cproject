using CourseProject.Models;
using Microsoft.AspNetCore.Identity;

namespace CourseProject.Services;

public class DataInitializer
{
    public static async Task SeedRolesAndAdmin(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roleNames = { "User", "Admin" };
        foreach (var roleName in roleNames)
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole(roleName));

        var adminEmail = "admin@example.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                Name = "Administrator"
            };
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded) await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}