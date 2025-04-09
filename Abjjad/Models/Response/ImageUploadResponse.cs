namespace Abjjad.Models.Response
{
    public class ImageUploadResponse
    {
        public string UniqueId { get; set; }
        public string OriginalFileName { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }
}
