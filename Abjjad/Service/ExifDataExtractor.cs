using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using Abjjad.Interface;
using Abjjad.Models;

public class ExifDataExtractor : IExifDataExtractor
{
    /// <summary>
    /// Extracts metadata from an image, including camera information and geolocation data
    /// </summary>
    /// <param name="image">The image to extract metadata from</param>
    /// <returns>Structured metadata object containing available EXIF data</returns>
    public ImageMetadata ExtractMetadata(Image image)
    {
        var metadata = image.Metadata;
        var result = new ImageMetadata();

        if (metadata.ExifProfile == null)
        {
            Console.WriteLine("No EXIF profile found.");
            return result;
        }

        Console.WriteLine("Available EXIF tags:");
        foreach (var exif in metadata.ExifProfile.Values)
        {
            Console.WriteLine($"{exif.Tag} = {exif.GetValue()}");
        }

        result.CameraMake = GetExifStringValue(metadata.ExifProfile, ExifTag.Make);
        result.CameraModel = GetExifStringValue(metadata.ExifProfile, ExifTag.Model);

        Console.WriteLine($"Make: {result.CameraMake}, Model: {result.CameraModel}");

        try
        {
            var gpsLatitude = GetExifRationalArray(metadata.ExifProfile, ExifTag.GPSLatitude);
            var gpsLatitudeRef = GetExifStringValue(metadata.ExifProfile, ExifTag.GPSLatitudeRef);
            var gpsLongitude = GetExifRationalArray(metadata.ExifProfile, ExifTag.GPSLongitude);
            var gpsLongitudeRef = GetExifStringValue(metadata.ExifProfile, ExifTag.GPSLongitudeRef);

            if (gpsLatitude != null && gpsLongitude != null && !string.IsNullOrEmpty(gpsLatitudeRef) && !string.IsNullOrEmpty(gpsLongitudeRef))
            {
                result.GeoLocation = new GeoLocation
                {
                    Latitude = ConvertGpsToDecimal(gpsLatitude, gpsLatitudeRef),
                    Longitude = ConvertGpsToDecimal(gpsLongitude, gpsLongitudeRef)
                };

                Console.WriteLine($"Latitude: {result.GeoLocation.Latitude}, Longitude: {result.GeoLocation.Longitude}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting GPS data: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Retrieves a string value from EXIF metadata
    /// </summary>
    /// <param name="profile">The EXIF profile</param>
    /// <param name="tag">The tag to retrieve</param>
    /// <returns>String value associated with the tag or null if not found</returns>
    private string GetExifStringValue(ExifProfile profile, ExifTag<string> tag)
    {
        if (profile == null) return null;
        if (profile.TryGetValue(tag, out var exifValue))
        {
            return exifValue.Value?.Trim();
        }
        return null;
    }

    /// <summary>
    /// Retrieves an array of rational values from EXIF metadata
    /// </summary>
    /// <param name="profile">The EXIF profile</param>
    /// <param name="tag">The tag to retrieve</param>
    /// <returns>Array of rational values associated with the tag or null if not found</returns>
    private Rational[] GetExifRationalArray(ExifProfile profile, ExifTag<Rational[]> tag)
    {
        if (profile == null) return null;
        if (profile.TryGetValue(tag, out var exifValue))
        {
            return exifValue.Value;
        }
        return null;
    }

    /// <summary>
    /// Converts GPS coordinates from degrees/minutes/seconds format to decimal degrees
    /// </summary>
    /// <param name="coordinates">Array of rational values representing degrees, minutes, seconds</param>
    /// <param name="refValue">Reference value (N/S/E/W) indicating direction</param>
    /// <returns>GPS coordinate in decimal degrees format</returns>
    private double ConvertGpsToDecimal(Rational[] coordinates, string refValue)
    {
        if (coordinates == null || coordinates.Length < 3)
            return 0;

        double degrees = coordinates[0].Numerator / (double)coordinates[0].Denominator;
        double minutes = coordinates[1].Numerator / (double)coordinates[1].Denominator;
        double seconds = coordinates[2].Numerator / (double)coordinates[2].Denominator;

        double decimalValue = degrees + (minutes / 60.0) + (seconds / 3600.0);

        if (refValue == "S" || refValue == "W")
        {
            decimalValue = -decimalValue;
        }

        return decimalValue;
    }
}