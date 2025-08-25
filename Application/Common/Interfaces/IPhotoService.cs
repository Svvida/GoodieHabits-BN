namespace Application.Common.Interfaces
{
    public interface IPhotoService
    {
        Task<string> UploadPhotoAsync(Stream fileStream, string fileName, int accountId);
    }
}
