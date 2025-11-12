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
    }
}
