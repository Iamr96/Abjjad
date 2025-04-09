namespace Abjjad.Models
{
    public class StorageSettings
    {
        public string RootPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "storage");
    }
}
