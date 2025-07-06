using CourseProject.Models;
using CourseProject.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CourseProject.Services;

public interface IFormResultService
{
    Task<List<FormResultViewModel>> GetFormResultsAsync(int templateId);
}

public class FormResultService : IFormResultService
{
    private readonly AppDbContext _context;

    public FormResultService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<FormResultViewModel>> GetFormResultsAsync(int templateId)
    {
        var forms = await _context.Forms
            .Include(f => f.User)
            .Include(f => f.Answers)
            .ThenInclude(a => a.Question)
            .Where(f => f.TemplateId == templateId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        return forms.Select(f => new FormResultViewModel
        {
            FormId = f.Id,
            UserEmail = f.User?.Email,
            CreatedAt = f.CreatedAt,
            Answers = f.Answers
                .Where(a => a.Question.IsVisibleInResults)
                .OrderBy(a => a.Question.Order)
                .Select(a => new FormAnswerViewModel
                {
                    QuestionTitle = a.Question.Title,
                    Value = a.Value
                }).ToList()
        }).ToList();
    }
}