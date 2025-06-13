using CourseProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseProject.Services;

public class TemplateService : ITemplateService
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICloudinaryService _cloudinaryService;

    public TemplateService(AppDbContext context, UserManager<ApplicationUser> userManager, ICloudinaryService cloudinaryService)
    {
        _context = context;
        _userManager = userManager;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<List<TemplateViewModel>> GetTemplatesAsync(bool isAuthenticated, string userId)
    {
        var query = BuildTemplateQuery(isAuthenticated);
        return await MapToViewModels(query);
    }

    public async Task<TemplateViewModel?> GetTemplateAsync(int id, bool isAuthenticated, string userId)
    {
        var template = await FetchTemplate(id);
        if (!CanAccessTemplate(template, isAuthenticated)) return null;
        return MapToViewModel(template);
    }

    public async Task CreateTemplateAsync(TemplateViewModel model, string userId, IFormFile? image)
    {
        var template = MapToTemplate(model, userId);
        template.Tags = ParseTags(model.Tags);
        if (image != null) template.ImageUrl = await _cloudinaryService.UploadImageAsync(image);
        await SaveTemplate(template);
    }

    public async Task UpdateTemplateAsync(TemplateViewModel model, string userId)
    {
        var template = await FetchTemplate(model.Id) ?? throw new KeyNotFoundException();
        if (!await IsOwnerOrAdminAsync(model.Id, userId)) throw new UnauthorizedAccessException();
        UpdateTemplate(template, model);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteTemplateAsync(int id, string userId)
    {
        var template = await FetchTemplate(id) ?? throw new KeyNotFoundException();
        if (!await IsOwnerOrAdminAsync(id, userId)) throw new UnauthorizedAccessException();
        _context.Templates.Remove(template);
        await _context.SaveChangesAsync();
    }

    public async Task<List<string>> GetTagsAsync(string term)
    {
        return await _context.Templates
            .SelectMany(t => t.Tags)
            .Where(t => t.StartsWith(term, StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .Take(10)
            .ToListAsync();
    }

    public async Task<bool> IsOwnerOrAdminAsync(int templateId, string userId)
    {
        var template = await FetchTemplate(templateId);
        if (template == null) return false;
        var user = await _userManager.FindByIdAsync(userId);
        return template.OwnerId == userId || await _userManager.IsInRoleAsync(user, "Admin");
    }

    private IQueryable<Template> BuildTemplateQuery(bool isAuthenticated)
    {
        var query = _context.Templates.Include(t => t.Owner);
        return isAuthenticated ? query : query.Where(t => t.IsPublic);
    }

    private async Task<List<TemplateViewModel>> MapToViewModels(IQueryable<Template> query)
    {
        return await query.Select(t => new TemplateViewModel
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            Theme = t.Theme,
            IsPublic = t.IsPublic,
            OwnerName = t.Owner.Name,
            Tags = string.Join(", ", t.Tags)
        }).ToListAsync();
    }

    private async Task<Template?> FetchTemplate(int id)
    {
        return await _context.Templates
            .Include(t => t.Owner)
            //.Include(t => t.Questions)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    private static bool CanAccessTemplate(Template? template, bool isAuthenticated)
    {
        return template != null && (template.IsPublic || isAuthenticated);
    }

    private static TemplateViewModel MapToViewModel(Template template)
    {
        return new TemplateViewModel
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            Theme = template.Theme,
            IsPublic = template.IsPublic,
            ImageUrl = template.ImageUrl,
            OwnerName = template.Owner.Name,
            Tags = string.Join(", ", template.Tags),
            //Questions = template.Questions
        };
    }

    private static Template MapToTemplate(TemplateViewModel model, string userId)
    {
        return new Template
        {
            Name = model.Name,
            Description = model.Description,
            Theme = model.Theme,
            IsPublic = model.IsPublic,
            OwnerId = userId
        };
    }

    private static List<string> ParseTags(string? tags)
    {
        return tags?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .ToList() ?? new List<string>();
    }

    private void UpdateTemplate(Template template, TemplateViewModel model)
    {
        template.Name = model.Name;
        template.Description = model.Description;
        template.Theme = model.Theme;
        template.IsPublic = model.IsPublic;
        template.Tags = ParseTags(model.Tags);
    }

    private async Task SaveTemplate(Template template)
    {
        _context.Templates.Add(template);
        await _context.SaveChangesAsync();
    }
}