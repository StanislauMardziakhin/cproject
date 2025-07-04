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

    public async Task<(bool, string)> AddQuestionAsync(int templateId, Question question, string userId, bool isAdmin = false)
    {
        var template = await _templateService.GetForEditAsync(templateId, userId, isAdmin);
        if (template == null)
            return (false, "TemplateNotFoundOrAccessDenied");
        if (await ExceedsMaxQuestionsOfType(templateId, question.Type))
            return (false, "MaxQuestionsPerType");
        question.TemplateId = templateId;
        question.Template = null;
        question.Order = await GetNextOrderForTemplate(templateId);
        _context.Questions.Add(question);
        return await TrySaveChangesAsync();
    }

    public async Task<Question?> GetQuestionAsync(int id, string userId, bool isAdmin)
    {
        return await _context.Questions
            .Include(q => q.Template)
            .Where(q => q.Id == id && (isAdmin || (q.Template != null && q.Template.UserId == userId)))
            .FirstOrDefaultAsync();
    }

    public async Task<(bool, string)> UpdateQuestionAsync(int id, Question updatedQuestion, string userId, bool isAdmin)
    {
        var existingQuestion = await GetQuestionAsync(id, userId, isAdmin);
        if (existingQuestion == null)
            return (false, "QuestionNotFoundOrAccessDenied");
        if (await ExceedsMaxQuestionsOfType(existingQuestion.TemplateId, updatedQuestion.Type, id))
            return (false, "MaxQuestionsPerType");
        UpdateQuestionFields(existingQuestion, updatedQuestion);
        return await TrySaveChangesAsync();
    }

    public async Task<(bool, string)> DeleteQuestionAsync(int id, string userId, bool isAdmin = false)
    {
        var question = await GetQuestionAsync(id, userId, isAdmin);
        if (question == null)
            return (false, "QuestionNotFoundOrAccessDenied");
        _context.Questions.Remove(question);
        return await TrySaveChangesAsync();
    }

    public async Task<(bool, string)> UpdateQuestionOrderAsync(List<QuestionOrder> orders, string userId, bool isAdmin = false)
    {
        var questionIds = orders.Select(o => o.Id).ToList();
        var questions = await _context.Questions
            .Include(q => q.Template)
            .Where(q => questionIds.Contains(q.Id) && (isAdmin || (q.Template != null && q.Template.UserId == userId)))
            .ToListAsync();
        if (questions.Count != orders.Count)
            return (false, "QuestionNotFoundOrAccessDenied");
        UpdateQuestionOrders(questions, orders);
        return await TrySaveChangesAsync();
    }

    public async Task<List<Question>> GetQuestionsAsync(int templateId, string? userId = null, bool isAdmin = false)
    {
        var query = _context.Questions
            .Where(q => q.TemplateId == templateId);
        if (!isAdmin && userId != null)
            query = query.Where(q => (q.Template != null && q.Template.UserId == userId) || (q.Template != null && q.Template.IsPublic));
        return await query
            .OrderBy(q => q.Order)
            .ToListAsync();
    }

    private async Task<bool> ExceedsMaxQuestionsOfType(int templateId, QuestionType type, int? excludeId = null)
    {
        var query = _context.Questions
            .Where(q => q.TemplateId == templateId && q.Type == type);
        if (excludeId.HasValue)
            query = query.Where(q => q.Id != excludeId.Value);
        var count = await query.CountAsync();
        return count >= 4;
    }

    private async Task<int> GetNextOrderForTemplate(int templateId)
    {
        var maxOrder = await _context.Questions
            .Where(q => q.TemplateId == templateId)
            .Select(q => (int?)q.Order)
            .MaxAsync();
        return (maxOrder ?? 0) + 1;
    }

    private static void UpdateQuestionFields(Question existing, Question updated)
    {
        existing.Title = updated.Title;
        existing.Description = updated.Description;
        existing.Type = updated.Type;
        existing.IsVisibleInResults = updated.IsVisibleInResults;
    }

    private static void UpdateQuestionOrders(List<Question> questions, List<QuestionOrder> orders)
    {
        foreach (var question in questions)
        {
            var order = orders.First(o => o.Id == question.Id).Order;
            question.Order = order;
        }
    }

    private async Task<(bool, string)> TrySaveChangesAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            return (true, string.Empty);
        }
        catch
        {
            return (false, "DatabaseError");
        }
    }
}

public class QuestionOrder
{
    public int Id { get; set; }
    public int Order { get; set; }
}