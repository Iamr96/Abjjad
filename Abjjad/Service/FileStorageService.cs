using Abjjad.Interface;
using Abjjad.Models;
using System.Text.Json;
public class FileStorageService : IFileStorageService
{
    private readonly string _storageRoot;
    private readonly ILogger<FileStorageService> _logger;

    /// <summary>
    /// Initializes the file storage service and ensures required directories exist
    /// </summary>
    public FileStorageService(IConfiguration configuration, ILogger<FileStorageService> logger)
    {
        _storageRoot = configuration["Storage:RootPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "storage");
        _logger = logger;
        EnsureDirectoryExists(Path.Combine(_storageRoot, "originals"));
        EnsureDirectoryExists(Path.Combine(_storageRoot, "resized"));
        EnsureDirectoryExists(Path.Combine(_storageRoot, "metadata"));
    }

    /// <summary>
    /// Stores the original image in WebP format
    /// </summary>
    /// <param name="imageStream">Source image data stream</param>
    /// <param name="uniqueId">Unique identifier for the image</param>
    public async Task StoreOriginalImage(Stream imageStream, string uniqueId)
    {
        var path = Path.Combine(_storageRoot, "originals", $"{uniqueId}.webp");
        await using var fileStream = new FileStream(path, FileMode.Create);
        await imageStream.CopyToAsync(fileStream);
    }

    /// <summary>
    /// Stores multiple resized versions of an image
    /// </summary>
    /// <param name="resizedImages">Dictionary of size names to image data</param>
    /// <param name="uniqueId">Unique identifier for the image</param>
    public async Task StoreResizedImages(Dictionary<string, MemoryStream> resizedImages, string uniqueId)
    {
        var resizedDir = Path.Combine(_storageRoot, "resized", uniqueId);
        Directory.CreateDirectory(resizedDir);
        foreach (var (size, stream) in resizedImages)
        {
            var path = Path.Combine(resizedDir, $"{size}.webp");
            await using var fileStream = new FileStream(path, FileMode.Create);
            await stream.CopyToAsync(fileStream);
        }
    }

    /// <summary>
    /// Stores metadata for an image as JSON
    /// </summary>
    /// <param name="metadata">Image metadata object</param>
    /// <param name="uniqueId">Unique identifier for the image</param>
    public async Task StoreMetadata(ImageMetadata metadata, string uniqueId)
    {
        var path = Path.Combine(_storageRoot, "metadata", $"{uniqueId}.json");
        var json = JsonSerializer.Serialize(metadata);
        await System.IO.File.WriteAllTextAsync(path, json);
    }

    /// <summary>
    /// Gets the file path for a specific size of a resized image
    /// </summary>
    /// <param name="uniqueId">Unique identifier for the image</param>
    /// <param name="size">Size variant identifier</param>
    /// <returns>Full file path to the resized image</returns>
    public string GetResizedImagePath(string uniqueId, string size)
    {
        return Path.Combine(_storageRoot, "resized", uniqueId, $"{size}.webp");
    }

    /// <summary>
    /// Retrieves the metadata for an image by its unique ID
    /// </summary>
    /// <param name="uniqueId">Unique identifier for the image</param>
    /// <returns>Image metadata object or null if not found</returns>
    public ImageMetadata GetMetadata(string uniqueId)
    {
        var path = Path.Combine(_storageRoot, "metadata", $"{uniqueId}.json");

        _logger.LogDebug($"Checking metadata path: {path}");

        if (!File.Exists(path))
        {
            _logger.LogWarning($"Metadata file not found at: {path}");
            return null;
        }

        try
        {
            var json = File.ReadAllText(path);
            _logger.LogDebug($"Raw metadata JSON: {json}");

            if (string.IsNullOrWhiteSpace(json))
            {
                _logger.LogWarning($"Empty metadata file found at: {path}");
                return null;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };

            var result = JsonSerializer.Deserialize<ImageMetadata>(json, options);

            if (result == null)
            {
                _logger.LogError($"Failed to deserialize metadata from: {path}");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error reading metadata file at: {path}");
            return null;
        }
    }

    /// <summary>
    /// Creates a directory if it doesn't already exist
    /// </summary>
    /// <param name="path">Directory path to ensure exists</param>
    private void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            _logger.LogInformation($"Created directory: {path}");
        }
    }
}