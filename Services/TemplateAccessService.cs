using CourseProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseProject.Services;

public class TemplateAccessService
{
    private readonly AppDbContext _context;

    public TemplateAccessService(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddUserAccessAsync(int templateId, string userId)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
            throw new Exception("User not found");

        var templateExists = await _context.Templates.AnyAsync(t => t.Id == templateId);
        if (!templateExists)
            throw new Exception("Template not found");

        var alreadyExists = await _context.TemplateAccesses
            .AnyAsync(a => a.TemplateId == templateId && a.UserId == userId);

        if (alreadyExists)
            return;

        var access = new TemplateAccess
        {
            TemplateId = templateId,
            UserId = userId
        };

        _context.TemplateAccesses.Add(access);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveUserAccessAsync(int templateId, string userId)
    {
        var access = await _context.TemplateAccesses
            .FirstOrDefaultAsync(ta => ta.TemplateId == templateId && ta.UserId == userId);
        if (access != null)
        {
            _context.TemplateAccesses.Remove(access);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<ApplicationUser>> GetUsersWithAccessAsync(int templateId)
    {
        return await _context.TemplateAccesses
            .Where(ta => ta.TemplateId == templateId)
            .Include(ta => ta.User)
            .Select(ta => ta.User)
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    public async Task<List<ApplicationUser>> SearchUsersAsync(string query)
    {
        return await _context.Users
            .Where(u => u.Name.Contains(query) || u.Email.Contains(query))
            .OrderBy(u => u.Name)
            .Take(10)
            .ToListAsync();
    }
} 