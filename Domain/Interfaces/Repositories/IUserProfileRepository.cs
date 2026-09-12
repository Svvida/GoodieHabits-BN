using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;
using Domain.ValueObjects;

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
        /// <summary>
        /// Just the timezone and the maintenance watermark, so the common case — the pass has already run
        /// today — costs one narrow query instead of loading every quest to discover there is nothing to do.
        /// </summary>
        Task<MaintenanceWatermark> GetMaintenanceWatermarkAsync(int userProfileId, CancellationToken cancellationToken = default);

        /// <summary>
        /// The profile with everything <see cref="UserProfile.RunDailyMaintenance"/> touches: quests with
        /// their periods and statistics, plus goals.
        /// </summary>
        Task<UserProfile?> GetProfileForMaintenanceAsync(int userProfileId, CancellationToken cancellationToken = default);

        /// <summary>Profiles whose maintenance pass has not run recently — the startup safety net's input.</summary>
        Task<IEnumerable<UserProfile>> GetProfilesNeedingMaintenanceAsync(DateOnly utcToday, CancellationToken cancellationToken = default);
        Task<UserProfile?> GetUserProfileWithBadgesAsync(int userProfileId, CancellationToken cancellationToken = default);
        IQueryable<UserProfile> SearchUserProfilesByNickname(string? nickname);
        Task<UserProfile?> GetUserProfileByIdForPublicDisplayAsync(int viewedUserProfileId, CancellationToken cancellationToken = default);
        Task<UserProfile?> GetUserProfileWithInventoryItemsForShopContextAsync(int userProfileId, bool asNoTracking = true, CancellationToken cancellationToken = default);
        Task<UserProfile?> GetUserProfileForAvatarUploadAsync(int userProfileId, CancellationToken cancellationToken = default);
    }
}
