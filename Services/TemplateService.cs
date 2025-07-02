using System.Text.RegularExpressions;
using CourseProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace CourseProject.Services;

public class TemplateService
{
    private readonly CloudinaryService _cloudinary;
    private readonly AppDbContext _context;
    private readonly IStringLocalizer<SharedResources> _localizer;
    private const string DefaultImageUrl = "/images/default.png";

    public TemplateService(AppDbContext context, CloudinaryService cloudinary, IStringLocalizer<SharedResources> localizer)
    {
        _context = context;
        _cloudinary = cloudinary;
        _localizer = localizer;
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
        if (string.IsNullOrWhiteSpace(userId)) return (false, _localizer["UserIdRequired"].Value);
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
        if (template == null) return (false, _localizer["TemplateNotFoundOrAccessDenied"].Value);
        UpdateTemplateFields(template, updated);
        await SetImageUrlAsync(template, image);
        await SaveTemplateAsync(template);
        return (true, string.Empty);
    }

    public async Task<(bool, string)> DeleteAsync(int id, string userId)
    {
        var template = await GetForEditAsync(id, userId);
        if (template == null) return (false, _localizer["TemplateNotFoundOrAccessDenied"].Value);
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
            return (false, _localizer["TemplateNotFoundOrAccessDenied"].Value);

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
        if (template == null) return (false, _localizer["TemplateNotFoundOrAccessDenied"].Value);
        if (!isAdmin && (userId == null || template.UserId != userId))
            return (false, _localizer["AccessDenied"].Value);
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
        {
            var imageUrl = await _cloudinary.UploadImageAsync(image);
            Console.WriteLine($"SetImageUrlAsync: Image provided, ImageUrl={imageUrl}");
            template.ImageUrl = string.IsNullOrEmpty(imageUrl) ? DefaultImageUrl : imageUrl;
        }
        else
        {
            Console.WriteLine($"SetImageUrlAsync: No image provided, setting ImageUrl={DefaultImageUrl}");
            template.ImageUrl = DefaultImageUrl;
        }
    }

    private async Task SaveTemplateAsync(Template template)
    {
        if (template.Id == 0) _context.Templates.Add(template);
        await _context.SaveChangesAsync();
    }

    public async Task<Template?> GetPublicTemplateAsync(int id, string? userId = null, bool isAdmin = false)
    {
        var query = _context.Templates
            .Include(t => t.Questions)
            .Include(t => t.User)
            .Include(t => t.Likes)
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
    public async Task<List<string>> SearchTagsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return [];
        query = query.ToLower().Trim();

        var tags = await _context.Templates
            .Where(t => t.Tags != null && t.Tags.ToLower().Contains(query))
            .Select(t => t.Tags)
            .Distinct()
            .ToListAsync();

        var uniqueTags = tags
            .SelectMany(t => t.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            .Where(t => t.ToLower().Contains(query))
            .Distinct()
            .Take(10) 
            .ToList();
        
        return uniqueTags;
    }
    public async Task<List<Template>> GetLatestTemplatesAsync(int count = 6)
    {
        return await _context.Templates
            .Include(t => t.User)
            .Where(t => t.IsPublic)
            .OrderByDescending(t => t.Id)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Template>> GetTopTemplatesByFormsAsync(int count = 5)
    {
        return await _context.Templates
            .Include(t => t.User)
            .Include(t => t.Forms)
            .Where(t => t.IsPublic)
            .OrderByDescending(t => t.Forms.Count)
            .ThenBy(t => t.Name)
            .Take(count)
            .ToListAsync();
    }
}