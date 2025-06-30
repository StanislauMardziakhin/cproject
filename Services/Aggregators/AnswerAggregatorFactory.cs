using CourseProject.Models;

namespace CourseProject.Services.Aggregators;

public class AnswerAggregatorFactory
{
    private readonly Dictionary<QuestionType, IAnswerAggregator> _aggregators;

    public AnswerAggregatorFactory()
    {
        _aggregators = new Dictionary<QuestionType, IAnswerAggregator>
        {
            { QuestionType.Integer, new IntegerAnswerAggregator() },
            { QuestionType.Checkbox, new CheckboxAnswerAggregator() },
            { QuestionType.String, new TextAnswerAggregator() },
            { QuestionType.Text, new TextAnswerAggregator() }
        };
    }

    public IAnswerAggregator GetAggregator(QuestionType type)
    {
        if (_aggregators.TryGetValue(type, out var aggregator))
            return aggregator;

        throw new NotSupportedException($"No aggregator found for question type {type}");
    }
}