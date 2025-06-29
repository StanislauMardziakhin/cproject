namespace CourseProject.Models;

public class Form
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public Template? Template { get; set; }
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<FormAnswer> Answers { get; set; } = [];
}

public class FormAnswer
{
    public int Id { get; set; }
    public int FormId { get; set; }
    public Form Form { get; set; }
    public int QuestionId { get; set; }
    public Question Question { get; set; }
    public string Value { get; set; } = string.Empty;
}