using CourseProject.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseProject.Services;

public interface ITemplateService
{
    Task<List<TemplateViewModel>> GetTemplatesAsync(bool isAuthenticated, string userId);
    Task<TemplateViewModel?> GetTemplateAsync(int id, bool isAuthenticated, string userId);
    Task CreateTemplateAsync(TemplateViewModel model, string userId, IFormFile? image);
    Task UpdateTemplateAsync(TemplateViewModel model, string userId);
    Task DeleteTemplateAsync(int id, string userId);
    Task<List<string>> GetTagsAsync(string term);
    Task<bool> IsOwnerOrAdminAsync(int templateId, string userId);
}