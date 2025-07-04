using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using CourseProject;

namespace CourseProject.ViewModels;

public class PasswordRequirementsAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var localizer = validationContext.GetService(typeof(IStringLocalizer<SharedResources>)) as IStringLocalizer<SharedResources>;
        string errorMessage = localizer?["PasswordRequirements"] ?? "Password must contain at least one uppercase letter, one digit, and one special character.";

        var password = value as string;
        if (string.IsNullOrEmpty(password))
            return ValidationResult.Success;
        
        if (!Regex.IsMatch(password, @"[A-Z]"))
            return new ValidationResult(errorMessage);
        if (!Regex.IsMatch(password, @"\d"))
            return new ValidationResult(errorMessage);
        if (!Regex.IsMatch(password, "[!@#$%^&*(),.?\":{}|<>]"))
            return new ValidationResult(errorMessage);

        return ValidationResult.Success;
    }
} 