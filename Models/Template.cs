using System.ComponentModel.DataAnnotations;
using NpgsqlTypes;

namespace CourseProject.Models;

public class Template
{
    public int Id { get; set; }
    [Required, StringLength(50)]
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    [Required]
    public string Theme { get; set; } = "Other";
    public bool IsPublic { get; set; } = true;
    public string? ImageUrl { get; set; }
    [Required]
    public string OwnerId { get; set; } = string.Empty;
    public ApplicationUser? Owner { get; set; }
    public List<string> Tags { get; set; } = new();
    //public List<Question> Questions { get; set; } = new();
    public NpgsqlTsVector SearchVector { get; set; }
}