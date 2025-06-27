using System.Text.RegularExpressions;
using CourseProject.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseProject.Services;

public class TemplateService
{
    private readonly CloudinaryService _cloudinary;
    private readonly AppDbContext _context;

    public TemplateService(AppDbContext context, CloudinaryService cloudinary)
    {
        _context = context;
        _cloudinary = cloudinary;
    }

    public async Task<List<Template>> GetUserTemplatesAsync(string userId, bool? isPublic = null)
    {
        var query = _context.Templates.AsQueryable();
        if (string.IsNullOrEmpty(userId) && isPublic == true)
            query = query.Where(t => t.IsPublic);
        else if (!string.IsNullOrEmpty(userId)) query = query.Where(t => t.UserId == userId);
        return await query.ToListAsync();
    }

    public async Task<(bool, string)> CreateAsync(Template template, IFormFile? image, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return (false, "UserIdRequired");
        template.UserId = userId;
        await SetImageUrlAsync(template, image);
        await SaveTemplateAsync(template);
        return (true, string.Empty);
    }

    public async Task<Template?> GetForEditAsync(int id, string? userId, bool isAdmin = false)
    {
        var query = _context.Templates
            .Include(t => t.Questions)
            .Where(t => t.Id == id);

        if (!isAdmin) query = query.Where(t => t.UserId == userId);

        return await query.FirstOrDefaultAsync();
    }

    public async Task<(bool, string)> UpdateAsync(int id, Template updated, IFormFile? image, string userId,
        bool isAdmin = false)
    {
        var template = await GetForEditAsync(id, userId, isAdmin);
        if (template == null) return (false, "TemplateNotFoundOrAccessDenied");
        UpdateTemplateFields(template, updated);
        await SetImageUrlAsync(template, image);
        await SaveTemplateAsync(template);
        return (true, string.Empty);
    }

    public async Task<(bool, string)> DeleteAsync(int id, string userId)
    {
        var template = await GetForEditAsync(id, userId);
        if (template == null) return (false, "TemplateNotFoundOrAccessDenied");
        _context.Templates.Remove(template);
        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }

    public async Task<(bool, string)> ApplyMassActionAsync(string action, int[] templateIds, string userId,
        bool isAdmin)
    {
        var results = new List<(bool, string)>();
        var actionHandlers = new Dictionary<string, Func<int, Task<(bool, string)>>>
        {
            ["delete"] = id => DeleteAsync(id, userId),
            ["publish"] = id => UpdatePublicStatusAsync(id, true, userId, isAdmin),
            ["unpublish"] = id => UpdatePublicStatusAsync(id, false, userId, isAdmin)
        };

        foreach (var id in templateIds)
        {
            var (succeeded, error) =
                await ProcessTemplateAction(id, isAdmin, userId, actionHandlers.GetValueOrDefault(action.ToLower()));
            results.Add((succeeded, error));
        }

        return (results.All(r => r.Item1), results.FirstOrDefault(r => !r.Item1).Item2 ?? string.Empty);
    }

    private async Task<(bool, string)> ProcessTemplateAction(int id, bool isAdmin, string userId,
        Func<int, Task<(bool, string)>>? actionHandler)
    {
        var template = await _context.Templates.FindAsync(id);
        if (template == null || (!isAdmin && template.UserId != userId))
            return (false, "TemplateNotFoundOrAccessDenied");

        if (actionHandler == null) return (false, "InvalidAction");
        return actionHandler.Method.Name switch
        {
            nameof(UpdatePublicStatusAsync) => await actionHandler(id),
            _ => await actionHandler(id)
        };
    }

    private async Task<(bool, string)> UpdatePublicStatusAsync(int id, bool isPublic, string? userId, bool isAdmin)
    {
        var template = await _context.Templates.FindAsync(id);
        if (template == null) return (false, "TemplateNotFoundOrAccessDenied");
        if (!isAdmin && (userId == null || template.UserId != userId))
            return (false, "AccessDenied");
        template.IsPublic = isPublic;
        return (await _context.SaveChangesAsync() > 0, string.Empty);
    }

    private bool IsValidUser(string userId)
    {
        return !string.IsNullOrEmpty(userId);
    }

    private void UpdateTemplateFields(Template template, Template updated)
    {
        template.Name = updated.Name;
        template.Description = updated.Description;
        template.Theme = updated.Theme;
        template.IsPublic = updated.IsPublic;
        template.Tags = updated.Tags;
    }

    private async Task SetImageUrlAsync(Template template, IFormFile? image)
    {
        if (image != null)
            template.ImageUrl = await _cloudinary.UploadImageAsync(image);
    }

    private async Task SaveTemplateAsync(Template template)
    {
        if (template.Id == 0) _context.Templates.Add(template);
        await _context.SaveChangesAsync();
    }

    public async Task<Template?> GetPublicTemplateAsync(int id, string? userId = null, bool isAdmin = false)
    {
        var query = _context.Templates.AsQueryable()
            .Where(t => t.Id == id && (isAdmin || t.IsPublic || (userId != null && t.UserId == userId)));
        return await query.FirstOrDefaultAsync();
    }

    public async Task<List<Template>> SearchAsync(string query, string culture = "en", string filter = "public",
        bool isAdmin = false)
    {
        if (string.IsNullOrWhiteSpace(query)) return [];
        query = Regex.Replace(query.ToLower(), @"[:\*&\|!']", "").Trim();
        if (string.IsNullOrEmpty(query))
            return [];

        var tsConfig = culture.StartsWith("es", StringComparison.OrdinalIgnoreCase) ? "spanish" : "english";
        var queryable = _context.Templates.AsQueryable();
        if (!isAdmin || string.Equals(filter, "public", StringComparison.OrdinalIgnoreCase))
            queryable = queryable.Where(t => t.IsPublic);
        else if (string.Equals(filter, "private", StringComparison.OrdinalIgnoreCase))
            queryable = queryable.Where(t => !t.IsPublic);

        return await queryable
            .Where(t => EF.Functions.ToTsVector(tsConfig,
                    (t.Name ?? "") + " " + (t.Description ?? "") + " " + (t.Tags ?? ""))
                .Matches(EF.Functions.ToTsQuery(tsConfig, query + ":*")))
            .ToListAsync();
    }
}