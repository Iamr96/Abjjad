using Abjjad.Interface;
using Abjjad.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

public class ImageProcessor : IImageProcessor
{
    private readonly IExifDataExtractor _exifExtractor;
    private readonly ILogger<ImageProcessor> _logger;

    /// <summary>
    /// Initializes the image processor with required dependencies
    /// </summary>
    /// <param name="exifExtractor">Service for extracting EXIF metadata from images</param>
    /// <param name="logger">Logger for recording processing operations</param>
    public ImageProcessor(IExifDataExtractor exifExtractor, ILogger<ImageProcessor> logger)
    {
        _exifExtractor = exifExtractor;
        _logger = logger;
    }

    /// <summary>
    /// Processes an image by extracting metadata, converting to WebP format, and generating resized versions
    /// </summary>
    /// <param name="imageStream">Input image stream</param>
    /// <param name="uniqueId">Unique identifier for the image</param>
    /// <returns>Processing result containing original and resized images with metadata</returns>
    public async Task<ImageProcessingResult> ProcessImage(Stream imageStream, string uniqueId)
    {
        try
        {
            // Load image
            using var image = await Image.LoadAsync(imageStream);

            // Extract metadata
            var metadata = _exifExtractor.ExtractMetadata(image);

            // Convert to WebP
            var originalWebP = new MemoryStream();
            await image.SaveAsWebpAsync(originalWebP);
            originalWebP.Position = 0;

            // Resize for different devices
            var resizedImages = new Dictionary<string, MemoryStream>
            {
                ["phone"] = await ResizeImage(image, 640, 1136),    // iPhone 5 size
                ["tablet"] = await ResizeImage(image, 1536, 2048), // iPad size
                ["desktop"] = await ResizeImage(image, 1920, 1080) // Full HD
            };

            return new ImageProcessingResult
            {
                OriginalImage = originalWebP,
                ResizedImages = resizedImages,
                Metadata = metadata
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image");
            throw;
        }
    }

    /// <summary>
    /// Resizes an image to specified dimensions while maintaining aspect ratio and converts to WebP format
    /// </summary>
    /// <param name="image">Source image</param>
    /// <param name="width">Target maximum width</param>
    /// <param name="height">Target maximum height</param>
    /// <returns>Memory stream containing the resized image in WebP format</returns>
    private async Task<MemoryStream> ResizeImage(Image image, int width, int height)
    {
        var clone = image.Clone(x => x.Resize(new ResizeOptions
        {
            Size = new Size(width, height),
            Mode = ResizeMode.Max
        }));

        var stream = new MemoryStream();
        await clone.SaveAsWebpAsync(stream);
        stream.Position = 0;
        return stream;
    }
}