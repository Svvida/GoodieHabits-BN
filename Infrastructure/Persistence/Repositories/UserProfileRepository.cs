using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class UserProfileRepository(AppDbContext context) : BaseRepository<UserProfile>(context), IUserProfileRepository
    {
        public async Task<bool> DoesNicknameExistAsync(string nickname, int accountId, CancellationToken cancellationToken = default)
        {
            return await _context.UserProfiles.AnyAsync(u => u.Nickname == nickname && u.AccountId != accountId, cancellationToken)
                .ConfigureAwait(false);
        }
        public async Task<bool> DoesNicknameExistAsync(string nickname, CancellationToken cancellationToken = default)
        {
            return await _context.UserProfiles.AnyAsync(u => u.Nickname == nickname, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<UserProfile?> GetByAccountIdAsync(int accountId, CancellationToken cancellationToken = default)
        {
            return await _context.UserProfiles
                .FirstOrDefaultAsync(u => u.AccountId == accountId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<UserProfile>> GetProfilesByAccountIdsAsync(IEnumerable<int> accountIds, CancellationToken cancellationToken = default)
        {
            return await _context.UserProfiles
                .Where(u => accountIds.Contains(u.AccountId))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<UserProfile?> GetUserProfileWithGoalsAsync(int accountId, CancellationToken cancellationToken = default)
        {
            return await _context.UserProfiles
                .AsNoTracking()
                .Include(u => u.Account)
                    .ThenInclude(a => a.UserGoals)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.AccountId == accountId, cancellationToken)
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
    }
}
