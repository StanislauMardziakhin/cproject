using CourseProject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseProject.Services
{
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
            if (template == null)
            {
                Console.WriteLine($"Template with ID {templateId} not found or access denied for user {userId}");
                return null;
            }

            return new Form
            {
                TemplateId = templateId,
                Template = template
            };
        }

        public async Task<(bool Success, string ErrorMessage)> SaveFormAsync(Form form, Dictionary<int, string> answers, string userId)
        {
            Console.WriteLine($"Saving form for TemplateId: {form.TemplateId}, UserId: {userId}, Answers count: {answers.Count}");
            var template = await _templateService.GetPublicTemplateAsync(form.TemplateId, userId, isAdmin: false);
            if (template == null)
            {
                Console.WriteLine($"Template with ID {form.TemplateId} not found or access denied");
                return (false, "TemplateNotFound");
            }

            var questionIds = template.Questions.Select(q => q.Id).ToHashSet();
            Console.WriteLine($"Valid Question IDs: {string.Join(", ", questionIds)}");
            if (answers.Keys.Any(key => !questionIds.Contains(key)))
            {
                Console.WriteLine($"Invalid question IDs detected: {string.Join(", ", answers.Keys.Except(questionIds))}");
                return (false, "InvalidQuestionIds");
            }

            form.UserId = userId;
            form.CreatedAt = DateTime.UtcNow;
            form.Answers = answers.Select(a => new FormAnswer
            {
                QuestionId = a.Key,
                Value = a.Value ?? string.Empty
            }).ToList();

            Console.WriteLine($"Adding form with {form.Answers.Count} answers");
            _context.Forms.Add(form);
            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine("Form saved successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving form: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return (false, $"DatabaseError: {ex.Message}");
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
    }
}