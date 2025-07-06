using CourseProject.Models;
using Microsoft.Extensions.Localization;

namespace CourseProject.Services.Aggregators;

public class AnswerAggregatorFactory
{
    private readonly Dictionary<QuestionType, IAnswerAggregator> _aggregators;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public AnswerAggregatorFactory(IStringLocalizer<SharedResources> localizer)
    {
        _localizer = localizer;
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

        throw new NotSupportedException($"{_localizer["NoAggregatorFound"].Value} {type}");
    }
}