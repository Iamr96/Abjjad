using System.ComponentModel.DataAnnotations;

namespace Abjjad.Models.Request
{
    public class ImageUploadRequest
    {
        [Required]
        public List<IFormFile> Files { get; set; }
    }
}
