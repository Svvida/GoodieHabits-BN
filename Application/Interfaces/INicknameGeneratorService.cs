namespace Application.Interfaces
{
    public interface INicknameGeneratorService
    {
        Task<string> GenerateUniqueNicknameAsync(CancellationToken cancellationToken = default);
    }
}
