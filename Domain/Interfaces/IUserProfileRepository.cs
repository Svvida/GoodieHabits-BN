using Domain.Models;

namespace Domain.Interfaces
{
    public interface IUserProfileRepository
    {
        Task<UserProfile?> GetByAccountIdAsync(int accountId, CancellationToken cancellationToken = default);
        Task<bool> ExistsByNicknameAsync(string nickname, CancellationToken cancellationToken = default);
        Task UpdateAsync(UserProfile userProfile, CancellationToken cancellationToken = default);
    }
}
