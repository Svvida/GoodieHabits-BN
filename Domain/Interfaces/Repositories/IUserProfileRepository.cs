using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IUserProfileRepository : IBaseRepository<UserProfile>
    {
        Task<bool> DoesNicknameExistAsync(string nickname, int accountId, CancellationToken cancellationToken = default);
        Task<bool> DoesNicknameExistAsync(string nickname, CancellationToken cancellationToken = default);
        Task<UserProfile?> GetUserProfileWithGoalsAsync(int accountId, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserProfile>> GetTenProfilesWithMostXpAsync(CancellationToken cancellationToken = default);
        Task<UserProfile?> GetUserProfileToWipeoutDataAsync(int accountId, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserProfile>> GetProfilesWithGoalsToExpireAsync(DateTime nowUtc, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserProfile>> GetProfilesWithQuestsToResetAsync(DateTime nowUtc, CancellationToken cancellationToken = default);
    }
}
