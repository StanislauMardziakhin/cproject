using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace CourseProject.Services;

public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];
        
        if (string.IsNullOrEmpty(cloudName))
            cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME");
        if (string.IsNullOrEmpty(apiKey))
            apiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY");
        if (string.IsNullOrEmpty(apiSecret))
            apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");
        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
    }
    
    public async Task<string> UploadImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0) return string.Empty;
        using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams { File = new FileDescription(file.FileName, stream), Folder = "course-project" };
        var result = await _cloudinary.UploadAsync(uploadParams);
        return result.SecureUrl.ToString();
    }
}