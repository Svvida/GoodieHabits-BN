using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
using Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserProfileRepository : BaseRepository<UserProfile>, IUserProfileRepository
    {
        public UserProfileRepository(AppDbContext context) : base(context) { }

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
    }
}
