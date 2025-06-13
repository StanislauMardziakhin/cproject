using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CourseProject.Services;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(IFormFile file);
}