namespace Application.Common.Interfaces
{
    public interface IUrlBuilder
    {
        string BuildProfilePageAvatarUrl(string? publicId);
        string BuildThumbnailAvatarUrl(string? publicId);
    }
}
