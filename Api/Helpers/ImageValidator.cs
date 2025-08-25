using Domain.Enums;
using SixLabors.ImageSharp;

namespace Api.Helpers
{
    public static class ImageValidator
    {
        public const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
        public const int MaxDimension = 4000; // 4000 pixels

        public static ImageValidationResult Validate(IFormFile file)
        {
            if (file is null || file.Length == 0)
                return ImageValidationResult.IsEmpty;

            if (file.Length > MaxFileSize)
                return ImageValidationResult.IsTooLarge;

            try
            {
                using var image = Image.Load(file.OpenReadStream());

                if (image.Width > MaxDimension || image.Height > MaxDimension)
                    return ImageValidationResult.InvalidDimensions;

                if (image.Metadata.DecodedImageFormat?.Name is not ("JPEG" or "PNG" or "WEBP"))
                    return ImageValidationResult.UnsupportedFormat;

                return ImageValidationResult.Valid;
            }
            catch (UnknownImageFormatException)
            {
                return ImageValidationResult.UnsupportedFormat;
            }
            finally
            {
                file.OpenReadStream().Position = 0; // Reset stream position after reading
            }
        }
    }
}
