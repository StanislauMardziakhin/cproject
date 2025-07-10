using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace CourseProject.ViewModels;

public class TemplateViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessageResourceName = "NameRequired", ErrorMessageResourceType = typeof(Resources.SharedResources))]
    [StringLength(50, ErrorMessageResourceName = "NameTooLong", ErrorMessageResourceType = typeof(Resources.SharedResources))]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessageResourceName = "RequiredField", ErrorMessageResourceType = typeof(Resources.SharedResources))]
    [StringLength(500, ErrorMessageResourceName = "DescriptionTooLong", ErrorMessageResourceType = typeof(Resources.SharedResources))]
    public string Description { get; set; } = string.Empty;

    [StringLength(50, ErrorMessageResourceName = "ThemeTooLong", ErrorMessageResourceType = typeof(Resources.SharedResources))]
    public string Theme { get; set; } = "Other";

    public bool IsPublic { get; set; }

    [StringLength(500, ErrorMessageResourceName = "TagsTooLong", ErrorMessageResourceType = typeof(Resources.SharedResources))]
    public string Tags { get; set; } = string.Empty;

    [StringLength(500, ErrorMessageResourceName = "ImageUrlTooLong", ErrorMessageResourceType = typeof(Resources.SharedResources))]
    public string ImageUrl { get; set; } = string.Empty;
    
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public int FormsCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public string? OwnerName { get; set; }
    public string UserId { get; set; } = string.Empty;
    public List<TemplateAccessViewModel> TemplateAccesses { get; set; } = [];
    public List<FormResultViewModel> Forms { get; set; } = [];
    public List<QuestionViewModel> QuestionsViewModel { get; set; } = [];
    public List<LikeViewModel> Likes { get; set; } = [];
    public List<CommentViewModel> Comments { get; set; } = new();
    public string DescriptionTruncated { get; set; } = string.Empty;
}

public class TemplateAccessViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
}

public class LikeViewModel
{
    public string UserId { get; set; } = string.Empty;
}

public class CommentViewModel
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}