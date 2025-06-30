using CourseProject.Models;
using CourseProject.ViewModels;

namespace CourseProject.Services.Aggregators;

public interface IAnswerAggregator
{
    QuestionAggregationResult Aggregate(Question question, List<string> answers);
}