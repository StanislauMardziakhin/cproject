using System.ComponentModel.DataAnnotations;

namespace CourseProject.ViewModels;

public class SalesforceViewModel
{
    [Required]
    [Display(Name = "AccountName", ResourceType = typeof(CourseProject.Resources.SharedResources))]
    public string AccountName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "ContactFirstName", ResourceType = typeof(CourseProject.Resources.SharedResources))]
    public string ContactFirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "ContactLastName", ResourceType = typeof(CourseProject.Resources.SharedResources))]
    public string ContactLastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "ContactEmail", ResourceType = typeof(CourseProject.Resources.SharedResources))]
    public string ContactEmail { get; set; } = string.Empty;
} 