using System.ComponentModel.DataAnnotations;

namespace CourseProject.Models;

public enum QuestionType
{
    [Display(Name = "String")] String,
    [Display(Name = "Text")] Text,
    [Display(Name = "Integer")] Integer,
    [Display(Name = "Checkbox")] Checkbox
}

public class Question
{
    public int Id { get; set; }

    [Required(ErrorMessage = "RequiredField")]
    public int TemplateId { get; set; }

    public Template? Template { get; set; }

    [Required(ErrorMessage = "RequiredField")]
    [StringLength(100, ErrorMessage = "TitleTooLong")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "DescriptionTooLong")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "RequiredField")]
    public QuestionType Type { get; set; }

    public int Order { get; set; }
    public bool IsVisibleInResults { get; set; } = true;
}