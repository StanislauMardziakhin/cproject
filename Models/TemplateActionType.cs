using System.ComponentModel.DataAnnotations;

namespace CourseProject.Models;

public enum TemplateActionType
{
    [Display(Name = "Publish Templates")] Publish,
    [Display(Name = "Unpublish Templates")] Unpublish,
    [Display(Name = "Delete Templates")] Delete
}