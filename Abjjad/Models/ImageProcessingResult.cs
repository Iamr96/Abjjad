namespace Abjjad.Models
{
    public class ImageProcessingResult
    {
        public Stream OriginalImage { get; set; }
        public Dictionary<string, MemoryStream> ResizedImages { get; set; }
        public ImageMetadata Metadata { get; set; }
    }
}
