using Domain.Interfaces.Repositories;
using Domain.Models;
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

        public async Task<IEnumerable<UserProfile>> GetProfilesWithQuestsToResetAsync(DateTime nowUtc, CancellationToken cancellationToken = default)
        {
            return await _context.UserProfiles
                .Include(a => a.Quests.Where(q => q.IsCompleted && (q.NextResetAt.HasValue && q.NextResetAt <= nowUtc) && ((q.EndDate ?? DateTime.MaxValue) > nowUtc)))
                .Where(a => a.Quests.Any(q => q.IsCompleted && (q.NextResetAt.HasValue && q.NextResetAt <= nowUtc) && ((q.EndDate ?? DateTime.MaxValue) > nowUtc)))
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
    }
}
