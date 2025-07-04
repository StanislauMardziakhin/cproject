using CourseProject.Models;

namespace CourseProject.ViewModels;

public class FormResultViewModel
{
    public int FormId { get; set; }
    public int TemplateId { get; set; }
    public string? TemplateName { get; set; }
    public string? UserEmail { get; set; }
    public string? UserName { get; set; }
    public string? UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<FormAnswerViewModel> Answers { get; set; } = [];
}

public class FormAnswerViewModel
{
    public int QuestionId { get; set; }
    public string QuestionTitle { get; set; } = string.Empty;
    public bool IsVisibleInResults { get; set; }
    public string Value { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; }
}