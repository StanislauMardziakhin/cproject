using System.ComponentModel.DataAnnotations;

namespace CourseProject.Models;

public class Comment
{
    public int Id { get; set; }

    [Required(ErrorMessage = "RequiredField")]
    public int TemplateId { get; set; }

    public Template? Template { get; set; }

    [Required(ErrorMessage = "RequiredField")]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    [Required(ErrorMessage = "RequiredField")]
    [StringLength(500, ErrorMessage = "CommentTooLong")]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}