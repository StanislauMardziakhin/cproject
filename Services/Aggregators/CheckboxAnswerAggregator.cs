using CourseProject.Models;
using CourseProject.ViewModels;

namespace CourseProject.Services.Aggregators;

public class CheckboxAnswerAggregator : IAnswerAggregator
{
    public QuestionAggregationResult Aggregate(Question question, List<string> answers)
    {
        return new QuestionAggregationResult
        {
            QuestionId = question.Id,
            QuestionTitle = question.Title,
            QuestionType = question.Type,
            TrueCount = answers.Count(v => v == "true"),
            FalseCount = answers.Count(v => v == "false")
        };
    }
}