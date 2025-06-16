using CourseProject.Models;
using Microsoft.AspNetCore.Identity;

namespace CourseProject.Services;

public class AccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<(bool Succeeded, List<string> Errors)> LoginAsync(string email, string password, bool rememberMe)
        {
            var errors = new List<string>();
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                errors.Add("User not found");
                return (false, errors);
            }

            var result = await _signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                errors.Add("Incorrect email or password");
                return (false, errors);
            }
            
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
            {
                await _signInManager.RefreshSignInAsync(user);
            }

            return (true, errors);
        }

        public async Task<(bool Succeeded, List<string> Errors)> RegisterAsync(string email, string password, string name)
        {
            var errors = new List<string>();
            var user = new ApplicationUser { UserName = email, Email = email, Name = name };
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                errors.AddRange(result.Errors.Select(e => e.Description));
                return (false, errors);
            }
            
            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
            {
                errors.AddRange(roleResult.Errors.Select(e => e.Description));
                return (false, errors);
            }
            
            await _signInManager.SignInAsync(user, isPersistent: false);
            return (true, errors);
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
}