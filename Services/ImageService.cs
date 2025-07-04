using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CourseProject.Services;

public class ImageService
{
    private readonly CloudinaryService _cloudinary;
    private const string DefaultImageUrl = "/images/default.png";

    public ImageService(CloudinaryService cloudinary)
    {
        _cloudinary = cloudinary;
    }

    public async Task<string> SetImageUrlAsync(IFormFile? image)
    {
        if (image != null)
        {
            var imageUrl = await _cloudinary.UploadImageAsync(image);
            return string.IsNullOrEmpty(imageUrl) ? DefaultImageUrl : imageUrl;
        }
        else
        {
            return DefaultImageUrl;
        }
    }
} 