using CourseProject.Hubs;
using CourseProject.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CourseProject.Services;

public class LikeService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<LikeHub> _hubContext;

    public LikeService(AppDbContext context, IHubContext<LikeHub> hubContext)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
    }

    public async Task<(bool succeeded, string error)> AddLikeAsync(int templateId, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return (false, "UserIdRequired");

        var template = await _context.Templates.FindAsync(templateId);
        if (template == null)
            return (false, "TemplateNotFound");

        var existingLike = await _context.Likes
            .FirstOrDefaultAsync(l => l.TemplateId == templateId && l.UserId == userId);
        if (existingLike != null)
            return (false, "LikeAlreadyExists");

        var like = new Like
        {
            TemplateId = templateId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Likes.Add(like);
        await _context.SaveChangesAsync();
        
            var likeCount = await _context.Likes.CountAsync(l => l.TemplateId == templateId);
            await _hubContext.Clients
                .Group($"template-{templateId}")
                .SendAsync("ReceiveLikeUpdate", new
            {
                templateId,
                likeCount
            });

        return (true, string.Empty);
    }

    public async Task<(bool succeeded, string error)> RemoveLikeAsync(int templateId, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return (false, "UserIdRequired");

        var like = await _context.Likes
            .FirstOrDefaultAsync(l => l.TemplateId == templateId && l.UserId == userId);
        if (like == null)
            return (false, "LikeNotFound");

        _context.Likes.Remove(like);
        await _context.SaveChangesAsync();
        
            var likeCount = await _context.Likes.CountAsync(l => l.TemplateId == templateId);
            await _hubContext.Clients
                .Group($"template-{templateId}")
                .SendAsync("ReceiveLikeUpdate", new
            {
                templateId,
                likeCount
            });
            
        return (true, string.Empty);
    }

    public async Task<int> GetLikeCountAsync(int templateId)
    {
        return await _context.Likes.CountAsync(l => l.TemplateId == templateId);
    }

    public async Task<bool> IsLikedAsync(int templateId, string userId)
    {
        return await _context.Likes
            .AnyAsync(l => l.TemplateId == templateId && l.UserId == userId);
    }
}