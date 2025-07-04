using System.ComponentModel.DataAnnotations;

namespace CourseProject.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessageResourceName = "RequiredField", ErrorMessageResourceType = typeof(Resources.SharedResources))]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessageResourceName = "RequiredField", ErrorMessageResourceType = typeof(Resources.SharedResources))]
    [DataType(DataType.Password)]
    [PasswordRequirements]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessageResourceName = "RequiredField", ErrorMessageResourceType = typeof(Resources.SharedResources))]
    public string Name { get; set; } = string.Empty;
}