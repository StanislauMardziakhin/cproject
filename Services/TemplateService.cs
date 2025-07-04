using System.Text.RegularExpressions;
using CourseProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Http;

namespace CourseProject.Services;

public class TemplateService
{
    private readonly CloudinaryService _cloudinary;
    private readonly AppDbContext _context;
    private readonly IStringLocalizer<SharedResources> _localizer;
    private readonly ImageService _imageService;
    private readonly TemplateAccessService _accessService;
    private readonly TemplatePublicationService _publicationService;
    private readonly TemplateSearchService _searchService;

    public TemplateService(AppDbContext context, CloudinaryService cloudinary,
        IStringLocalizer<SharedResources> localizer,
        ImageService imageService,
        TemplateAccessService accessService,
        TemplatePublicationService publicationService,
        TemplateSearchService searchService)
    {
        _context = context;
        _cloudinary = cloudinary;
        _localizer = localizer;
        _imageService = imageService;
        _accessService = accessService;
        _publicationService = publicationService;
        _searchService = searchService;
    }

    public async Task<List<Template>> GetUserTemplatesAsync(string userId, bool? isPublic = null)
    {
        var query = _context.Templates
            .Include(t => t.User)
            .Include(t => t.Likes)
            .Include(t => t.Comments)
            .Include(t => t.Forms)
            .Include(t => t.TemplateAccesses)
            .ThenInclude(ta => ta.User)
            .Where(t => t.UserId == userId);
        if (isPublic.HasValue)
            query = query.Where(t => t.IsPublic == isPublic.Value);
        return await query.ToListAsync();
    }

    public async Task<(bool, string)> CreateAsync(Template template, IFormFile? image, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return (false, _localizer["UserIdRequired"].Value);
        template.UserId = userId;
        template.ImageUrl = await _imageService.SetImageUrlAsync(image);
        await SaveTemplateAsync(template);
        return (true, string.Empty);
    }

    private IQueryable<Template> ApplyAccessFilter(IQueryable<Template> query, string? userId, bool isAdmin)
    {
        if (isAdmin)
            return query;

        if (string.IsNullOrEmpty(userId))
            return query.Where(t => t.IsPublic);

        return query.Where(t =>
            t.IsPublic ||
            t.UserId == userId ||
            t.TemplateAccesses.Any(a => a.UserId == userId));
    }

    public async Task<Template?> GetForEditAsync(int id, string? userId, bool isAdmin = false)
    {
        var query = _context.Templates
            .Include(t => t.Questions)
            .Include(t => t.TemplateAccesses)
            .ThenInclude(ta => ta.User)
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
        template.ImageUrl = await _imageService.SetImageUrlAsync(image);
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
            .Include(t => t.Comments)
            .ThenInclude(c => c.User)
            .Include(t => t.TemplateAccesses)
            .ThenInclude(ta => ta.User)
            .Where(t => t.Id == id && (isAdmin || t.IsPublic || (userId != null && t.UserId == userId) ||
                                       t.TemplateAccesses.Any(ta => ta.UserId == userId)));
        return await query.FirstOrDefaultAsync();
    }

    public async Task<List<Template>> GetLatestTemplatesAsync(string? userId, bool isAdmin = false, int count = 6)
    {
        IQueryable<Template> query = _context.Templates
            .Include(t => t.User)
            .Include(t => t.Forms)
            .Include(t => t.Likes)
            .Include(t => t.Comments)
            .Include(t => t.TemplateAccesses)
            .ThenInclude(ta => ta.User);

        query = ApplyAccessFilter(query, userId, isAdmin);

        return await query
            .OrderByDescending(t => t.Id)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Template>> GetTopTemplatesByFormsAsync(string? userId, bool isAdmin = false, int count = 5)
    {
        var query = _context.Templates
            .Include(t => t.User)
            .Include(t => t.Forms)
            .Include(t => t.Likes)
            .Include(t => t.Comments)
            .Include(t => t.TemplateAccesses)
            .ThenInclude(ta => ta.User)
            .AsQueryable();

        query = ApplyAccessFilter(query, userId, isAdmin);

        return await query
            .OrderByDescending(t => t.Forms.Count)
            .ThenBy(t => t.Name)
            .Take(count)
            .ToListAsync();
    }
    
    public Task AddUserAccessAsync(int templateId, string userId) => _accessService.AddUserAccessAsync(templateId, userId);
    public Task RemoveUserAccessAsync(int templateId, string userId) => _accessService.RemoveUserAccessAsync(templateId, userId);
    public Task<List<ApplicationUser>> GetUsersWithAccessAsync(int templateId) => _accessService.GetUsersWithAccessAsync(templateId);
    public Task<List<ApplicationUser>> SearchUsersAsync(string query) => _accessService.SearchUsersAsync(query);
    
    public Task<(bool, string)> ApplyMassActionAsync(string action, int[] templateIds, string userId, bool isAdmin)
        => _publicationService.ApplyMassActionAsync(action, templateIds, userId, isAdmin);
    public Task<(bool, string)> UpdatePublicStatusAsync(int id, bool isPublic, string? userId, bool isAdmin)
        => _publicationService.UpdatePublicStatusAsync(id, isPublic, userId, isAdmin);
    
    public Task<List<Template>> SearchAsync(string query, string culture = "en", string filter = "public", string? userId = null, bool isAdmin = false)
        => _searchService.SearchAsync(query, culture, filter, userId, isAdmin);
    public Task<List<string>> SearchTagsAsync(string query)
        => _searchService.SearchTagsAsync(query);
}