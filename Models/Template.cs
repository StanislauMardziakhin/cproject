using System.ComponentModel.DataAnnotations;

namespace CourseProject.Models;

public class Template
{
    public int Id { get; set; }

    [Required(ErrorMessage = "NameRequired")]
    [StringLength(50, ErrorMessage = "NameTooLong")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "DescriptionTooLong")]
    public string Description { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "ThemeTooLong")]
    public string Theme { get; set; } = "Other";

    public bool IsPublic { get; set; } = true;

    [StringLength(500, ErrorMessage = "TagsTooLong")]
    public string Tags { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "ImageUrlTooLong")]
    public string ImageUrl { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Question> Questions { get; set; } = [];
    public List<Form> Forms { get; set; } = [];

    public List<Comment> Comments { get; set; } = [];
}