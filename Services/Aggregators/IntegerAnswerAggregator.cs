using CourseProject.Models;
using CourseProject.ViewModels;

namespace CourseProject.Services.Aggregators;

public class IntegerAnswerAggregator : IAnswerAggregator
{
    public QuestionAggregationResult Aggregate(Question question, List<string> answers)
    {
        var ints = answers.Select(TryParseInt).Where(v => v.HasValue).Select(v => v.Value).ToList();
        var result = new QuestionAggregationResult
        {
            QuestionId = question.Id,
            QuestionTitle = question.Title,
            QuestionType = question.Type
        };

        if (ints.Any())
        {
            result.Min = ints.Min();
            result.Max = ints.Max();
            result.Average = ints.Average();
        }

        return result;
    }

    private int? TryParseInt(string s) => int.TryParse(s, out var v) ? v : null;
}