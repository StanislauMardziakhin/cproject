using CourseProject.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseProject.Services;

public class FormService
{
    private readonly AppDbContext _context;
    private readonly TemplateService _templateService;

    public FormService(AppDbContext context, TemplateService templateService)
    {
        _context = context;
        _templateService = templateService;
    }

    public async Task<Form?> GetFormForFillingAsync(int templateId, string userId, bool isAdmin)
    {
        var template = await _templateService.GetPublicTemplateAsync(templateId, userId, isAdmin);
        if (template == null) return null;

        return new Form
        {
            TemplateId = templateId,
            Template = template
        };
    }

    public async Task<(bool Success, string ErrorMessage)> SaveFormAsync(Form form, Dictionary<int, string> answers,
        string userId)
    {
        var template = await _templateService.GetPublicTemplateAsync(form.TemplateId, userId, false);
        if (template == null) return (false, "TemplateNotFoundOrAccessDenied");

        var questionIds = template.Questions.Select(q => q.Id).ToHashSet();
        if (answers.Keys.Any(key => !questionIds.Contains(key))) return (false, "InvalidQuestionIds");

        form.UserId = userId;
        form.CreatedAt = DateTime.UtcNow;
        form.Answers = answers.Select(a => new FormAnswer
        {
            QuestionId = a.Key,
            Value = a.Value ?? string.Empty
        }).ToList();

        _context.Forms.Add(form);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            if (ex.InnerException != null)
            {
            }

            return (false, "DatabaseError");
        }

        return (true, string.Empty);
    }

    public async Task<Form?> GetFormForViewingAsync(int formId, string userId, bool isAdmin)
    {
        var form = await _context.Forms
            .Include(f => f.Template)
            .ThenInclude(t => t.Questions)
            .Include(f => f.Answers)
            .ThenInclude(fa => fa.Question)
            .Include(f => f.User)
            .FirstOrDefaultAsync(f => f.Id == formId);

        if (form == null) return null;

        if (!isAdmin && form.UserId != userId && form.Template.UserId != userId)
            return null;

        return form;
    }

    public async Task<List<Form>> GetFormsForTemplateAsync(int templateId, string? userId, bool isAdmin)
    {
        var template = await _templateService.GetPublicTemplateAsync(templateId, userId, isAdmin);
        if (template == null) return new List<Form>();

        return await _context.Forms
            .Where(f => f.TemplateId == templateId)
            .Include(f => f.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<Form>> GetFormsForUserAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return new List<Form>();
        return await _context.Forms
            .Where(f => f.UserId == userId)
            .Include(f => f.Template)
            .ToListAsync();
    }

    public async Task<(bool Success, string ErrorKey)> DeleteFormsAsync(List<int> formIds, string userId, bool isAdmin)
    {
        var forms = await _context.Forms
            .Where(f => formIds.Contains(f.Id))
            .ToListAsync();

        if (!forms.Any())
            return (false, "ErrorFormsNotFound");

        if (!isAdmin)
            if (forms.Any(f => f.UserId != userId))
                return (false, "ErrorNoPermission");

        _context.Forms.RemoveRange(forms);
        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }
    public async Task<Form?> GetFormForEditingAsync(int formId, string userId, bool isAdmin)
    {
        var form = await _context.Forms
            .Include(f => f.Answers)
            .ThenInclude(a => a.Question)
            .Include(f => f.Template)
            .FirstOrDefaultAsync(f => f.Id == formId);

        if (form == null) return null;
        if (!isAdmin && form.UserId != userId) return null;

        return form;
    }

    public async Task<(bool Success, string ErrorKey)> EditFormAsync(int formId, Dictionary<int, string> updatedAnswers, string userId, bool isAdmin)
    {
        var form = await _context.Forms
            .Include(f => f.Answers)
            .Include(f => f.Template)
            .ThenInclude(t => t.Questions)
            .FirstOrDefaultAsync(f => f.Id == formId);

        if (form == null) return (false, "ErrorFormNotFound");
        if (!isAdmin && form.UserId != userId) return (false, "ErrorNoPermission");

        var validQuestionIds = form.Template.Questions.Select(q => q.Id).ToHashSet();

        foreach (var answer in form.Answers)
        {
            if (updatedAnswers.TryGetValue(answer.QuestionId, out var value) && validQuestionIds.Contains(answer.QuestionId))
            {
                answer.Value = value;
            }
        }

        try
        {
            await _context.SaveChangesAsync();
            return (true, string.Empty);
        }
        catch
        {
            return (false, "ErrorFormSave");
        }
    }
}