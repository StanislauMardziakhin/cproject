using CourseProject.Models;
using CourseProject.Services.Aggregators;
using CourseProject.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CourseProject.Services;

public interface IResultsAggregatorService
{
    Task<List<QuestionAggregationResult>> GetAggregatedResultsAsync(int templateId);
}

public class ResultsAggregatorService : IResultsAggregatorService
{
    private readonly AppDbContext _context;
    private readonly AnswerAggregatorFactory _factory;

    public ResultsAggregatorService(AppDbContext context)
    {
        _context = context;
        _factory = new AnswerAggregatorFactory();
    }

    public async Task<List<QuestionAggregationResult>> GetAggregatedResultsAsync(int templateId)
    {
        var questions = await _context.Questions
            .Where(q => q.TemplateId == templateId && q.IsVisibleInResults)
            .ToListAsync();

        var questionIds = questions.Select(q => q.Id).ToList();

        var answers = await _context.FormAnswers
            .Where(a => questionIds.Contains(a.QuestionId))
            .ToListAsync();

        var results = new List<QuestionAggregationResult>();

        foreach (var question in questions)
        {
            var questionAnswers = answers
                .Where(a => a.QuestionId == question.Id)
                .Select(a => a.Value)
                .ToList();

            var aggregator = _factory.GetAggregator(question.Type);
            var result = aggregator.Aggregate(question, questionAnswers);

            results.Add(result);
        }

        return results;
    }
}