using System.ComponentModel.DataAnnotations;

namespace CourseProject.Models;

public class Like
{
    public int Id { get; set; }

    [Required(ErrorMessageResourceName = "RequiredField", ErrorMessageResourceType = typeof(Resources.SharedResources))]
    public int TemplateId { get; set; }

    public Template? Template { get; set; }

    [Required(ErrorMessageResourceName = "RequiredField", ErrorMessageResourceType = typeof(Resources.SharedResources))]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}