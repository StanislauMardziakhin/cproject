using System.ComponentModel.DataAnnotations;

namespace CourseProject.Models;

public class Template
{
    public int Id { get; set; }

    [Required(ErrorMessageResourceName = "NameRequired", ErrorMessageResourceType = typeof(Resources.SharedResources))]
    [StringLength(50, ErrorMessageResourceName = "NameTooLong",
        ErrorMessageResourceType = typeof(Resources.SharedResources))]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessageResourceName = "RequiredField", ErrorMessageResourceType = typeof(Resources.SharedResources))]
    [StringLength(500, ErrorMessageResourceName = "DescriptionTooLong",
        ErrorMessageResourceType = typeof(Resources.SharedResources))]
    public string Description { get; set; } = string.Empty;

    [StringLength(50, ErrorMessageResourceName = "ThemeTooLong",
        ErrorMessageResourceType = typeof(Resources.SharedResources))]
    public string Theme { get; set; } = "Other";

    public bool IsPublic { get; set; }

    [StringLength(500, ErrorMessageResourceName = "TagsTooLong",
        ErrorMessageResourceType = typeof(Resources.SharedResources))]
    public string Tags { get; set; } = string.Empty;

    [StringLength(500, ErrorMessageResourceName = "ImageUrlTooLong",
        ErrorMessageResourceType = typeof(Resources.SharedResources))]
    public string ImageUrl { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Question> Questions { get; set; } = [];
    public List<Form> Forms { get; set; } = [];

    public List<Comment> Comments { get; set; } = [];
    public List<Like> Likes { get; set; } = [];
    public List<TemplateAccess> TemplateAccesses { get; set; } = [];
}