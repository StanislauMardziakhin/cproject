using System.ComponentModel.DataAnnotations;

namespace CourseProject.ViewModels;

public class RegisterViewModel
{
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required] public string Name { get; set; } = string.Empty;
}