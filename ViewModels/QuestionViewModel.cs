using System.ComponentModel.DataAnnotations;
using Resources = CourseProject.Resources;

namespace CourseProject.ViewModels;

public class QuestionViewModel
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    [Required(ErrorMessageResourceName = "RequiredField", ErrorMessageResourceType = typeof(Resources.SharedResources))]
    [StringLength(100, ErrorMessageResourceName = "TitleTooLong", ErrorMessageResourceType = typeof(Resources.SharedResources))]
    public string Title { get; set; } = string.Empty;
    [StringLength(500, ErrorMessageResourceName = "DescriptionTooLong", ErrorMessageResourceType = typeof(Resources.SharedResources))]
    public string Description { get; set; } = string.Empty;
    [Required(ErrorMessageResourceName = "RequiredField", ErrorMessageResourceType = typeof(Resources.SharedResources))]
    public string Type { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsVisibleInResults { get; set; }
} 