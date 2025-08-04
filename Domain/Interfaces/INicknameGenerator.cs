namespace Application.UserProfiles.Nickname
{
    public interface INicknameGenerator
    {
        Task<string> GenerateUniqueNicknameAsync(CancellationToken cancellationToken = default);
    }
}
