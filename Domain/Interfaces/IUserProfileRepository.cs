using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces
{
    public interface IUserProfileRepository : IBaseRepository<UserProfile>
    {
        Task<UserProfile?> GetByAccountIdAsync(int accountId, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserProfile>> GetProfilesByAccountIdsAsync(IEnumerable<int> accountIds, CancellationToken cancellationToken = default);
        Task<bool> DoesNicknameExistAsync(string nickname, int accountId, CancellationToken cancellationToken = default);
        Task<bool> DoesNicknameExistAsync(string nickname, CancellationToken cancellationToken = default);
        Task<UserProfile?> GetUserProfileWithGoalsAsync(int accountId, CancellationToken cancellationToken = default);
    }
}
