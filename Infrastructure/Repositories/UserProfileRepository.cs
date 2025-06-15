using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly AppDbContext _context;

        public UserProfileRepository(AppDbContext context)
        {
            _context = context;
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

        public async Task UpdateAsync(UserProfile userProfile, CancellationToken cancellationToken = default)
        {
            _context.UserProfiles.Update(userProfile);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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
