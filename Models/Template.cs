namespace CourseProject.Models;

public class Template
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Theme { get; set; } = "Other";
    public bool IsPublic { get; set; } = true;
}