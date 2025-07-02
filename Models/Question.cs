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

    [Required(ErrorMessageResourceName = "RequiredField", ErrorMessageResourceType = typeof(Resources.SharedResources))]
    public int TemplateId { get; set; }

    public Template? Template { get; set; }

    [Required(ErrorMessageResourceName = "RequiredField", ErrorMessageResourceType = typeof(Resources.SharedResources))]
    [StringLength(100, ErrorMessageResourceName = "TitleTooLong", ErrorMessageResourceType = typeof(Resources.SharedResources))]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessageResourceName = "DescriptionTooLong", ErrorMessageResourceType = typeof(Resources.SharedResources))]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessageResourceName = "RequiredField", ErrorMessageResourceType = typeof(Resources.SharedResources))]
    public QuestionType Type { get; set; }

    public int Order { get; set; }
    public bool IsVisibleInResults { get; set; } = true;
}