namespace Application.Common.Interfaces
{
    public interface IUrlBuilder
    {
        string BuildProfilePageAvatarUrl(string? publicId);
        string BuildThumbnailAvatarUrl(string? publicId);
        string BuildCosmeticUrl(string? publicId); // For frames, effects (400x400)
        string BuildPetUrl(string? publicId);      // For pets (150x150)
        string BuildShopItemThumbnailUrl(string? publicId); // Shop Grid (250x250)
    }
}
