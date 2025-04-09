using Abjjad.Models;

namespace Abjjad.Interface
{
    public interface IImageProcessor
    {
        Task<ImageProcessingResult> ProcessImage(Stream imageStream, string uniqueId);
    }
}
