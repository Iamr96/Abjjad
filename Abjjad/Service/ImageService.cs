using Abjjad.Interface;
using Abjjad.Models.Response;
using Abjjad.Models;

/// <summary>
/// Service for handling image processing, storage, and retrieval
/// </summary>
public class ImageService : IImageService
{
    private readonly IFileStorageService _storageService;
    private readonly IImageProcessor _imageProcessor;
    private readonly ILogger<ImageService> _logger;

    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private readonly long _maxFileSize = 2_000_000; // 2MB

    /// <summary>
    /// Initializes the image service with required dependencies
    /// </summary>
    /// <param name="storageService">Service for file storage operations</param>
    /// <param name="imageProcessor">Service for image processing operations</param>
    /// <param name="logger">Logger for recording service operations</param>
    public ImageService(
        IFileStorageService storageService,
        IImageProcessor imageProcessor,
        ILogger<ImageService> logger)
    {
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        _imageProcessor = imageProcessor ?? throw new ArgumentNullException(nameof(imageProcessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes an uploaded image file, validates it, generates resized versions, and stores all artifacts
    /// </summary>
    /// <param name="file">The uploaded image file</param>
    /// <returns>Response containing processing status and image identifiers</returns>
    public async Task<ImageUploadResponse> ProcessUploadedImage(IFormFile file)
    {
        var response = new ImageUploadResponse
        {
            OriginalFileName = file.FileName,
            Success = false
        };

        try
        {
            if (file == null)
            {
                response.Error = "No file was uploaded";
                return response;
            }

            if (file.Length <= 0)
            {
                response.Error = "Uploaded file is empty";
                return response;
            }

            if (file.Length > _maxFileSize)
            {
                response.Error = $"File size exceeds {_maxFileSize / 1_000_000}MB limit";
                return response;
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!_allowedExtensions.Contains(extension))
            {
                response.Error = $"Invalid file format. Only {string.Join(", ", _allowedExtensions)} are allowed";
                return response;
            }

            if (!IsValidImageMimeType(file.ContentType))
            {
                response.Error = "Invalid image content type";
                return response;
            }

            // Generate unique ID
            response.UniqueId = Guid.NewGuid().ToString();

            // Process image
            using var stream = file.OpenReadStream();
            var processingResult = await _imageProcessor.ProcessImage(stream, response.UniqueId);

            // Validate processing result
            if (processingResult == null || processingResult.OriginalImage == null ||
                processingResult.ResizedImages == null || processingResult.ResizedImages.Count == 0)
            {
                response.Error = "Error processing image: Invalid processing result";
                return response;
            }

            // Store files
            await _storageService.StoreOriginalImage(processingResult.OriginalImage, response.UniqueId);
            await _storageService.StoreResizedImages(processingResult.ResizedImages, response.UniqueId);
            await _storageService.StoreMetadata(processingResult.Metadata, response.UniqueId);

            response.Success = true;
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing image {file?.FileName ?? "unknown"}");
            response.Error = "Error processing image";
            return response;
        }
    }

    /// <summary>
    /// Gets the file path for a specific size of a resized image
    /// </summary>
    /// <param name="uniqueImageId">Unique identifier for the image</param>
    /// <param name="size">Size variant identifier (e.g., "phone", "tablet", "desktop")</param>
    /// <returns>File path to the resized image</returns>
    public string GetResizedImagePath(string uniqueImageId, string size)
    {
        // Validate parameters
        if (string.IsNullOrEmpty(uniqueImageId))
        {
            throw new ArgumentException("Image ID cannot be null or empty", nameof(uniqueImageId));
        }

        if (string.IsNullOrEmpty(size))
        {
            throw new ArgumentException("Size cannot be null or empty", nameof(size));
        }

        return _storageService.GetResizedImagePath(uniqueImageId, size);
    }

    /// <summary>
    /// Retrieves metadata for a specific image
    /// </summary>
    /// <param name="uniqueImageId">Unique identifier for the image</param>
    /// <returns>Image metadata or null if not found</returns>
    public ImageMetadata GetImageMetadata(string uniqueImageId)
    {
        // Validate parameter
        if (string.IsNullOrEmpty(uniqueImageId))
        {
            throw new ArgumentException("Image ID cannot be null or empty", nameof(uniqueImageId));
        }

        return _storageService.GetMetadata(uniqueImageId);
    }

    /// <summary>
    /// Validates if the content type represents a valid image format
    /// </summary>
    /// <param name="contentType">MIME content type</param>
    /// <returns>True if valid image content type, false otherwise</returns>
    private bool IsValidImageMimeType(string contentType)
    {
        if (string.IsNullOrEmpty(contentType))
        {
            return false;
        }

        var validContentTypes = new[]
        {
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/webp"
        };

        return validContentTypes.Contains(contentType.ToLower());
    }
}