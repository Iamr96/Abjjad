using Abjjad.Models;
using SixLabors.ImageSharp;


namespace Abjjad.Interface
{
    public interface IExifDataExtractor
    {
        ImageMetadata ExtractMetadata(Image image);
    }
}
