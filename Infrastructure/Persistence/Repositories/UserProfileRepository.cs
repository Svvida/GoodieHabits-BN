using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Domain.ValueObjects;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class UserProfileRepository(AppDbContext context) : BaseRepository<UserProfile>(context), IUserProfileRepository
    {
        public async Task<bool> DoesNicknameExistAsync(string nickname, int userProfileId, CancellationToken cancellationToken = default)
        {
            return await _context.UserProfiles.AnyAsync(u => u.Nickname == nickname && u.Id != userProfileId, cancellationToken)
                .ConfigureAwait(false);
        }
        public async Task<bool> DoesNicknameExistAsync(string nickname, CancellationToken cancellationToken = default)
        {
            return await _context.UserProfiles.AnyAsync(u => u.Nickname == nickname, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<UserProfile?> GetUserProfileWithGoalsAsync(int userProfileId, CancellationToken cancellationToken = default)
        {
            return await _context.UserProfiles
                .AsNoTracking()
                .Include(u => u.UserGoals)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userProfileId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<UserProfile>> GetTenProfilesWithMostXpAsync(CancellationToken cancellationToken = default)
        {
            return await _context.UserProfiles
                .OrderByDescending(u => u.TotalXp)
                .Take(10)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<UserProfile?> GetUserProfileToWipeoutDataAsync(int userProfileId, CancellationToken cancellationToken = default)
        {
            return await _context.UserProfiles
                .Include(u => u.Account)
                .Include(u => u.Labels)
                .Include(u => u.UserProfile_Badges)
                .Include(u => u.Quests)
                .Include(u => u.Notifications)
                .Include(u => u.SentFriendInvitations)
                .Include(u => u.ReceivedFriendInvitations)
                .Include(u => u.SentBlocks)
                .Include(u => u.FriendshipsAsUser1)
                .Include(u => u.FriendshipsAsUser2)
                .Include(u => u.InventoryItems)
                .Include(u => u.ActiveUserEffects)
                .Include(u => u.FinanceCategories)
                .Include(u => u.FinanceTransactions)
                .Include(u => u.Budgets)
                .FirstOrDefaultAsync(u => u.Id == userProfileId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<UserProfile>> GetProfilesWithGoalsToExpireAsync(DateTime nowUtc, CancellationToken cancellationToken = default)
        {
            return await _context.UserProfiles
                .Include(a => a.UserGoals.Where(ug => ug.EndsAt <= nowUtc && !ug.IsExpired))
                .Where(ug => ug.UserGoals.Any(ug => ug.EndsAt <= nowUtc && !ug.IsExpired))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<MaintenanceWatermark> GetMaintenanceWatermarkAsync(int userProfileId, CancellationToken cancellationToken = default)
        {
            return await _context.UserProfiles
                .AsNoTracking()
                .Where(up => up.Id == userProfileId)
                .Select(up => new MaintenanceWatermark(up.TimeZone, up.MaintainedThrough))
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<UserProfile?> GetProfileForMaintenanceAsync(int userProfileId, CancellationToken cancellationToken = default)
        {
            return await _context.UserProfiles
                .Where(up => up.Id == userProfileId)
                // Every occurrence of every quest: generation de-duplicates in memory against this
                // collection, and a partial load would emit a row that collides with the unique index.
                .Include(up => up.Quests).ThenInclude(q => q.QuestOccurrences)
                .Include(up => up.Quests).ThenInclude(q => q.Statistics)
                // Needed for the recalculated TotalCompletions; without it every quest reports zero taps.
                .Include(up => up.Quests).ThenInclude(q => q.Completions)
                .Include(up => up.UserGoals)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<UserProfile>> GetProfilesNeedingMaintenanceAsync(DateOnly utcToday, CancellationToken cancellationToken = default)
        {
            // Widened by a day because each user's local date may lead or trail the server's; the domain
            // re-checks against the real local date and no-ops if it is already up to date.
            var earliest = utcToday.AddDays(-1);

            return await _context.UserProfiles
                .Where(up => up.MaintainedThrough == null || up.MaintainedThrough < earliest)
                .Include(up => up.Quests).ThenInclude(q => q.QuestOccurrences)
                .Include(up => up.Quests).ThenInclude(q => q.Statistics)
                .Include(up => up.Quests).ThenInclude(q => q.Completions)
                .Include(up => up.UserGoals)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<UserProfile?> GetUserProfileWithBadgesAsync(int userProfileId, CancellationToken cancellationToken = default)
        {
            return await _context.UserProfiles
                .Include(up => up.UserProfile_Badges)
                    .ThenInclude(upb => upb.Badge)
                .FirstOrDefaultAsync(up => up.Id == userProfileId, cancellationToken)
                .ConfigureAwait(false);
        }

        public IQueryable<UserProfile> SearchUserProfilesByNickname(string? nickname)
        {
            var query = _context.UserProfiles.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(nickname))
            {
                query = query.Where(u => u.Nickname.Contains(nickname));
            }

            return query;
        }

        public async Task<UserProfile?> GetUserProfileByIdForPublicDisplayAsync(int viewedUserProfileId, CancellationToken cancellationToken = default)
        {
            // The handler will figure out what's important. We will fetch related data later.
            return await _context.UserProfiles
                .AsNoTracking()
                .Where(u => u.Id == viewedUserProfileId)
                .Include(u => u.UserProfile_Badges).ThenInclude(upb => upb.Badge)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<UserProfile?> GetUserProfileWithInventoryItemsForShopContextAsync(int userProfileId, bool asNoTracking = true, CancellationToken cancellationToken = default)
        {
            var query = _context.UserProfiles.AsQueryable();

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query
                .Include(up => up.InventoryItems)
                .FirstOrDefaultAsync(up => up.Id == userProfileId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<UserProfile?> GetUserProfileForAvatarUploadAsync(int userProfileId, CancellationToken cancellationToken = default)
        {
            return await _context.UserProfiles
                .Include(up => up.InventoryItems.Where(ii => ii.IsActive && ii.ShopItem.Category == ShopItemsCategoryEnum.Avatars))
                    .ThenInclude(ii => ii.ShopItem)
                .FirstOrDefaultAsync(up => up.Id == userProfileId, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
