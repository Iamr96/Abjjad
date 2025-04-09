using Abjjad.Models.Response;
using Abjjad.Models;

namespace Abjjad.Interface
{
    public interface IImageService
    {
        Task<ImageUploadResponse> ProcessUploadedImage(IFormFile file);
        string GetResizedImagePath(string uniqueImageId, string size);
        ImageMetadata GetImageMetadata(string uniqueImageId);
    }
}
