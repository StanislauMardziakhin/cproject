using CourseProject.Models;

namespace CourseProject.ViewModels;

public class QuestionAggregationResult
{
    public int QuestionId { get; set; }
    public string QuestionTitle { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; }

    public double? Average { get; set; }
    public int? Min { get; set; }
    public int? Max { get; set; }

    public string? MostCommonAnswer { get; set; }

    public int TrueCount { get; set; }
    public int FalseCount { get; set; }
}