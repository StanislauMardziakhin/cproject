using CourseProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseProject.Services;

public class TemplatePublicationService
{
    private readonly AppDbContext _context;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public TemplatePublicationService(AppDbContext context, IStringLocalizer<SharedResources> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<(bool, string)> ApplyMassActionAsync(string action, int[] templateIds, string userId, bool isAdmin)
    {
        var results = new List<(bool, string)>();
        var actionHandlers = new Dictionary<string, Func<int, Task<(bool, string)>>> {
            ["publish"] = id => UpdatePublicStatusAsync(id, true, userId, isAdmin),
            ["unpublish"] = id => UpdatePublicStatusAsync(id, false, userId, isAdmin),
            ["delete"] = async id => {
                var template = await _context.Templates.FindAsync(id);
                if (template == null || (!isAdmin && template.UserId != userId))
                    return (false, _localizer["TemplateNotFoundOrAccessDenied"].Value);
                _context.Templates.Remove(template);
                await _context.SaveChangesAsync();
                return (true, string.Empty);
            }
        };
        foreach (var id in templateIds)
        {
            var (succeeded, error) = await ProcessTemplateAction(id, isAdmin, userId, actionHandlers.GetValueOrDefault(action.ToLower()));
            results.Add((succeeded, error));
        }
        return (results.All(r => r.Item1), results.FirstOrDefault(r => !r.Item1).Item2 ?? string.Empty);
    }

    private async Task<(bool, string)> ProcessTemplateAction(int id, bool isAdmin, string userId, Func<int, Task<(bool, string)>>? actionHandler)
    {
        var template = await _context.Templates.FindAsync(id);
        if (template == null || (!isAdmin && template.UserId != userId))
            return (false, _localizer["TemplateNotFoundOrAccessDenied"].Value);
        if (actionHandler == null) return (false, "InvalidAction");
        return await actionHandler(id);
    }

    public async Task<(bool, string)> UpdatePublicStatusAsync(int id, bool isPublic, string? userId, bool isAdmin)
    {
        var template = await _context.Templates.FindAsync(id);
        if (template == null) return (false, _localizer["TemplateNotFoundOrAccessDenied"].Value);
        if (!isAdmin && (userId == null || template.UserId != userId))
            return (false, _localizer["AccessDenied"].Value);
        template.IsPublic = isPublic;
        return (await _context.SaveChangesAsync() > 0, string.Empty);
    }
} 