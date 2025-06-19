using CourseProject.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseProject.Services;

public class TemplateService
{
    private readonly AppDbContext _context;
    private readonly CloudinaryService _cloudinary;

    public TemplateService(AppDbContext context, CloudinaryService cloudinary)
    {
        _context = context;
        _cloudinary = cloudinary;
    }
    
    public async Task<List<Template>> GetUserTemplatesAsync(string userId)
    {
        return await _context.Templates
            .Where(t => t.UserId == userId)
            .ToListAsync();
    }
    
    public async Task<(bool, string)> CreateAsync(Template template, IFormFile? image, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return (false, "UserIdRequired");
        template.UserId = userId;
        await SetImageUrlAsync(template, image);
        await SaveTemplateAsync(template);
        return (true, string.Empty);
    }
    
    public async Task<Template?> GetForEditAsync(int id, string userId)
    {
        var template = await _context.Templates.FindAsync(id);
        return template != null && template.UserId == userId ? template : null;
    }
    
    public async Task<(bool, string)> UpdateAsync(int id, Template updated, IFormFile? image, string userId)
    {
        var template = await GetForEditAsync(id, userId);
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
}