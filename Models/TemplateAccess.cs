using System.ComponentModel.DataAnnotations;

namespace CourseProject.Models;

public class TemplateAccess
{
    public int Id { get; set; }

    [Required]
    public int TemplateId { get; set; }

    [Required]
    public string UserId { get; set; }
    
    public Template Template { get; set; }
    
    public ApplicationUser User { get; set; }
}