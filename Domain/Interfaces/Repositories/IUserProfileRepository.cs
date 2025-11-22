using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IUserProfileRepository : IBaseRepository<UserProfile>
    {
        Task<bool> DoesNicknameExistAsync(string nickname, int userProfileId, CancellationToken cancellationToken = default);
        Task<bool> DoesNicknameExistAsync(string nickname, CancellationToken cancellationToken = default);
        Task<UserProfile?> GetUserProfileWithGoalsAsync(int userProfileId, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserProfile>> GetTenProfilesWithMostXpAsync(CancellationToken cancellationToken = default);
        Task<UserProfile?> GetUserProfileToWipeoutDataAsync(int userProfileId, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserProfile>> GetProfilesWithGoalsToExpireAsync(DateTime nowUtc, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserProfile>> GetProfilesWithQuestsToResetAsync(DateTime nowUtc, CancellationToken cancellationToken = default);
        Task<UserProfile?> GetUserProfileWithBadgesAsync(int userProfileId, CancellationToken cancellationToken = default);
        IQueryable<UserProfile> SearchUserProfilesByNickname(string? nickname);
        Task<UserProfile?> GetUserProfileByIdForPublicDisplayAsync(int viewedUserProfileId, CancellationToken cancellationToken = default);
        Task<UserProfile?> GetUserProfileWithInventoryItemsForShopContextAsync(int userProfileId, CancellationToken cancellationToken = default);
    }
}
