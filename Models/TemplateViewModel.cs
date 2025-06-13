using System.ComponentModel.DataAnnotations;

namespace CourseProject.Models;

public class TemplateViewModel(string name = "", string description = "", string theme = "Other", bool isPublic = true)
{
    public int Id { get; set; }
    [Required, StringLength(50)]
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    [Required]
    public string Theme { get; set; } = theme;
    public bool IsPublic { get; set; } = isPublic;
    public string? ImageUrl { get; set; }
    public string? Tags { get; set; }
    public string? OwnerName { get; set; }
    //public List<Question> Questions { get; set; } = new();
}