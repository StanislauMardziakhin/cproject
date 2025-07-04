using CourseProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseProject.Services;

public class TemplateSearchService
{
    private readonly AppDbContext _context;

    public TemplateSearchService(AppDbContext context)
    {
        _context = context;
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

    public async Task<List<Template>> SearchAsync(string query, string culture = "en", string filter = "public", string? userId = null, bool isAdmin = false)
    {
        if (string.IsNullOrWhiteSpace(query)) return new List<Template>();
        query = Regex.Replace(query.ToLower(), @"[:\*&\|!']", "").Trim();
        if (string.IsNullOrEmpty(query))
            return new List<Template>();
        
        var searchTerms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var formattedQuery = string.Join(" & ", searchTerms.Select(term => term + ":*"));
        
        var tsConfig = culture.StartsWith("es", StringComparison.OrdinalIgnoreCase) ? "spanish" : "english";
        var templates = _context.Templates
            .Include(t => t.User)
            .Include(t => t.Likes)
            .Include(t => t.Comments)
            .Include(t => t.TemplateAccesses);
        var filtered = ApplyAccessFilter(templates, userId, isAdmin);
        return await filtered
            .Where(t => EF.Functions.ToTsVector(tsConfig,
                    (t.Name ?? "") + " " + (t.Description ?? "") + " " + (t.Tags ?? ""))
                .Matches(EF.Functions.ToTsQuery(tsConfig, formattedQuery)))
            .ToListAsync();
    }

    public async Task<List<string>> SearchTagsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return new List<string>();
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
} 