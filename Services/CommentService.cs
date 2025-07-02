using CourseProject.Hubs;
using CourseProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CourseProject.Services;

public class CommentService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<CommentHub> _hubContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public CommentService(AppDbContext context, IHubContext<CommentHub> hubContext,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _hubContext = hubContext;
        _userManager = userManager;
    }

    public async Task<(bool succeeded, string error)> AddCommentAsync(int templateId, string content, string userId)
    {
        var template = await _context.Templates.FindAsync(templateId);
        if (template == null)
            return (false, "TemplateNotFound");

        var comment = new Comment
        {
            TemplateId = templateId,
            UserId = userId,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        var user = await _context.Users.FindAsync(userId);
        var userName = user?.Name;
        await _hubContext.Clients
            .Group($"template-{templateId}")
            .SendAsync("ReceiveComment", new
            {
                commentId = comment.Id,
                userName,
                content = comment.Content,
                createdAt = comment.CreatedAt,
                templateId = comment.TemplateId
            });
        return (true, string.Empty);
    }

    public async Task<(bool succeeded, string error)> DeleteCommentAsync(int commentId, string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            return (false, "Unauthorized");

        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == commentId);
        if (comment == null)
            return (false, "CommentNotFound");

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();

        await _hubContext.Clients
            .Group($"template-{comment.TemplateId}")
            .SendAsync("CommentDeleted", new
            {
                commentId = comment.Id,
                templateId = comment.TemplateId
            });

        return (true, string.Empty);
    }

    public async Task<List<Comment>> GetCommentsAsync(int templateId)
    {
        return await _context.Comments
            .Include(c => c.User)
            .Where(c => c.TemplateId == templateId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }
}