using Application.Common.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace Infrastructure.Photos
{
    public class CloudinaryPhotoService(Cloudinary cloudinary, IOptions<CloudinarySettings> settings) : IPhotoService
    {
        private readonly CloudinarySettings _settings = settings.Value;
        public async Task<string> UploadPhotoAsync(Stream fileStream, string fileName, int userProfileId)
        {
            var publicId = $"{userProfileId}";

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(fileName, fileStream),
                PublicId = publicId,
                Overwrite = true,
                Folder = _settings.AvatarsFolder,
                UniqueFilename = false,
                Invalidate = true,
                Transformation = new Transformation()
                    .Quality("auto").Width(400).Height(400).Crop("thumb").Gravity("face")
            };

            var uploadResult = await cloudinary.UploadAsync(uploadParams).ConfigureAwait(false);

            return uploadResult.PublicId;
        }
    }
}
