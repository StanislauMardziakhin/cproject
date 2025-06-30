using CourseProject.Models;
using CourseProject.ViewModels;

namespace CourseProject.Services.Aggregators;

public class TextAnswerAggregator : IAnswerAggregator
{
    public QuestionAggregationResult Aggregate(Question question, List<string> answers)
    {
        var mostCommon = answers
            .GroupBy(v => v)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        return new QuestionAggregationResult
        {
            QuestionId = question.Id,
            QuestionTitle = question.Title,
            QuestionType = question.Type,
            MostCommonAnswer = mostCommon
        };
    }
}