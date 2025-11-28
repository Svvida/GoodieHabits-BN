using Application.Common.Interfaces;
using CloudinaryDotNet;

namespace Infrastructure.Photos
{
    public class CloudinaryUrlBuilder(Cloudinary cloudinary) : IUrlBuilder
    {
        public string BuildProfilePageAvatarUrl(string? publicId)
        {
            if (string.IsNullOrEmpty(publicId))
            {
                return string.Empty;
            }

            return cloudinary.Api.Url.
                Transform(new Transformation()
                    .Width(400).Height(400)
                    .Crop("thumb").Gravity("face")
                    .FetchFormat("auto")
                    .Quality("auto"))
                .BuildUrl(publicId);
        }

        public string BuildThumbnailAvatarUrl(string? publicId)
        {
            if (string.IsNullOrEmpty(publicId))
            {
                return string.Empty;
            }

            return cloudinary.Api.Url
                .Transform(new Transformation()
                    .Width(100).Height(100)
                    .Crop("thumb").Gravity("face")
                    .FetchFormat("auto")
                    .Quality("auto"))
                .BuildUrl(publicId);
        }

        public string BuildCosmeticUrl(string? publicId)
        {
            if (string.IsNullOrEmpty(publicId))
                return string.Empty;

            // Frames/Effects need high res to look good over the avatar
            return cloudinary.Api.Url.Transform(new Transformation()
                .Width(400).Height(400)
                .FetchFormat("auto").Quality("auto")) // No Crop! Frames need full context
                .BuildUrl(publicId);
        }

        public string BuildPetUrl(string? publicId)
        {
            if (string.IsNullOrEmpty(publicId))
                return string.Empty;

            return cloudinary.Api.Url.Transform(new Transformation()
                .Width(150).Height(150)
                .Crop("fit") // Fit ensures the whole pet is visible
                .FetchFormat("auto").Quality("auto"))
                .BuildUrl(publicId);
        }
    }
}
