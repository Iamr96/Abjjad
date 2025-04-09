using Abjjad.Models;

namespace Abjjad.Interface
{
    public interface IFileStorageService
    {
        Task StoreOriginalImage(Stream imageStream, string uniqueId);
        Task StoreResizedImages(Dictionary<string, MemoryStream> resizedImages, string uniqueId);
        Task StoreMetadata(ImageMetadata metadata, string uniqueId);
        string GetResizedImagePath(string uniqueId, string size);
        ImageMetadata GetMetadata(string uniqueId);
    }
}
