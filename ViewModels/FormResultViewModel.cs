namespace CourseProject.ViewModels;

public class FormResultViewModel
{
    public int FormId { get; set; }
    public string? UserEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<FormAnswerViewModel> Answers { get; set; } = [];
}

public class FormAnswerViewModel
{
    public string QuestionTitle { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}