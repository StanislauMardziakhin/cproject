using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CourseProject.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        var account = CreateAccount(configuration);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        var uploadParams = CreateUploadParams(file);
        var result = await _cloudinary.UploadAsync(uploadParams);
        return result.SecureUrl.ToString();
    }

    private static Account CreateAccount(IConfiguration configuration)
    {
        return new Account(
            configuration[""],
            configuration[""],
            configuration[""]);
    }

    private static ImageUploadParams CreateUploadParams(IFormFile file)
    {
        return new ImageUploadParams
        {
            File = new FileDescription(file.FileName, file.OpenReadStream())
        };
    }
}