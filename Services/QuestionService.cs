using CourseProject.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseProject.Services;

public class QuestionService
{
    private readonly AppDbContext _context;
    private readonly TemplateService _templateService;

    public QuestionService(AppDbContext context, TemplateService templateService)
    {
        _context = context;
        _templateService = templateService;
    }

    public async Task<(bool, string)> AddQuestionAsync(int templateId, Question question, string userId,
        bool isAdmin = false)
    {
        var template = await _templateService.GetForEditAsync(templateId, userId, isAdmin);
        if (template == null)
            return (false, "TemplateNotFoundOrAccessDenied");

        var questionCount = await _context.Questions
            .Where(q => q.TemplateId == templateId && q.Type == question.Type)
            .CountAsync();
        if (questionCount >= 4)
            return (false, "MaxQuestionsPerType");

        question.TemplateId = templateId;
        question.Template = null;
        var maxOrder = await _context.Questions
            .Where(q => q.TemplateId == templateId)
            .Select(q => (int?)q.Order)
            .MaxAsync() ?? 0;

        question.Order = maxOrder + 1;

        _context.Questions.Add(question);
        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }

    public async Task<Question?> GetQuestionAsync(int id, string userId, bool isAdmin)
    {
        return await _context.Questions
            .Include(q => q.Template)
            .Where(q => q.Id == id && (isAdmin || q.Template.UserId == userId))
            .FirstOrDefaultAsync();
    }

    public async Task<(bool, string)> UpdateQuestionAsync(int id, Question updatedQuestion, string userId, bool isAdmin)
    {
        var existingQuestion = await GetQuestionAsync(id, userId, isAdmin);
        if (existingQuestion == null)
            return (false, "QuestionNotFoundOrAccessDenied");

        var questionCount = await _context.Questions
            .Where(q => q.TemplateId == existingQuestion.TemplateId && q.Type == updatedQuestion.Type && q.Id != id)
            .CountAsync();
        if (questionCount >= 4)
            return (false, "MaxQuestionsPerType");

        existingQuestion.Title = updatedQuestion.Title;
        existingQuestion.Description = updatedQuestion.Description;
        existingQuestion.Type = updatedQuestion.Type;
        existingQuestion.IsVisibleInResults = updatedQuestion.IsVisibleInResults;

        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }

    public async Task<(bool, string)> DeleteQuestionAsync(int id, string userId, bool isAdmin = false)
    {
        var question = await GetQuestionAsync(id, userId, isAdmin);
        if (question == null)
            return (false, "QuestionNotFoundOrAccessDenied");

        _context.Questions.Remove(question);
        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }

    public async Task<(bool, string)> UpdateQuestionOrderAsync(List<QuestionOrder> orders, string userId,
        bool isAdmin = false)
    {
        var questionIds = orders.Select(o => o.Id).ToList();
        var questions = await _context.Questions
            .Include(q => q.Template)
            .Where(q => questionIds.Contains(q.Id) && (isAdmin || q.Template.UserId == userId))
            .ToListAsync();

        if (questions.Count != orders.Count)
            return (false, "QuestionNotFoundOrAccessDenied");

        foreach (var question in questions)
        {
            var order = orders.First(o => o.Id == question.Id).Order;
            question.Order = order;
        }

        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }

    public async Task<List<Question>> GetQuestionsAsync(int templateId, string? userId = null, bool isAdmin = false)
    {
        var query = _context.Questions
            .Where(q => q.TemplateId == templateId);

        if (!isAdmin && userId != null)
            query = query.Where(q => q.Template.UserId == userId || q.Template.IsPublic);

        return await query
            .OrderBy(q => q.Order)
            .ToListAsync();
    }
}

public class QuestionOrder
{
    public int Id { get; set; }
    public int Order { get; set; }
}