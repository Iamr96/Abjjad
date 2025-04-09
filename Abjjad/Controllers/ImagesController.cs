// ImageController.cs
using Abjjad.Interface;
using Abjjad.Models.Request;
using Abjjad.Models.Response;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

[ApiController]
[EnableCors("AllowAll")]  
[Route("api/[controller]")]
public class ImagesController : ControllerBase
{
    private readonly IImageService _imageService;
    private readonly ILogger<ImagesController> _logger;

    public ImagesController(IImageService imageService, ILogger<ImagesController> logger)
    {
        _imageService = imageService;
        _logger = logger;
    }

    [HttpPost]
    [RequestSizeLimit(20_000_000)] // 20MB total for multiple images
    public async Task<ActionResult<IEnumerable<ImageUploadResponse>>> UploadImages([FromForm] ImageUploadRequest request)
    {
        try
        {
            if (request.Files == null || request.Files.Count == 0)
            {
                return BadRequest("No images provided");
            }

            var results = new List<ImageUploadResponse>();

            foreach (var file in request.Files)
            {
                var result = await _imageService.ProcessUploadedImage(file);
                results.Add(result);
            }

            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading images");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    [HttpGet("{uniqueImageId}/{size}")]
    public IActionResult GetImage(string uniqueImageId, string size)
    {
        try
        {
            // Check if named size or numeric
            var allowedSizes = new[] { "phone", "tablet", "desktop" };
            if (!allowedSizes.Contains(size.ToLower()) && !double.TryParse(size, out _))
            {
                return BadRequest("Invalid size parameter. Must be phone, tablet, desktop or numeric value");
            }

            var imagePath = _imageService.GetResizedImagePath(uniqueImageId, size);
            if (!System.IO.File.Exists(imagePath))
            {
                return NotFound();
            }

            var imageBytes = System.IO.File.ReadAllBytes(imagePath);
            return File(imageBytes, "image/webp");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving image {uniqueImageId} for size {size}");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }




    [HttpGet("{uniqueImageId}/metadata")]
    public IActionResult GetMetadata(string uniqueImageId)
    {
        try
        {
            var metadata = _imageService.GetImageMetadata(uniqueImageId);
            if (metadata == null)
            {
                return NotFound();
            }

            return Ok(metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving metadata for image {uniqueImageId}");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

}
