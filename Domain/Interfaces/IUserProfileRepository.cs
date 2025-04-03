namespace Domain.Interfaces
{
    public interface IUserProfileRepository
    {
        Task<bool> ExistsByNicknameAsync(string nickname, CancellationToken cancellationToken = default);
    }
}
