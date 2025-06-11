using Microsoft.AspNetCore.Identity;

namespace CourseProject.Models;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; } = string.Empty;
}