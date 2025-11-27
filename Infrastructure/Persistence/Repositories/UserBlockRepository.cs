using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class UserBlockRepository(AppDbContext context) : BaseRepository<UserBlock>(context), IUserBlockRepository
    {
        public async Task<bool> IsUserBlockExistsByProfileIdsAsync(int blockerUserProfileId, int blockedUserProfileId, CancellationToken cancellationToken = default)
        {
            return await _context.UserBlocks.AnyAsync(ub =>
                ub.BlockerUserProfileId == blockerUserProfileId &&
                ub.BlockedUserProfileId == blockedUserProfileId, cancellationToken).ConfigureAwait(false);
        }

        public async Task<UserBlock?> GetUserBlockByProfileIdsAsync(int blockerUserProfileId, int blockedUserProfileId, bool loadProfiles, CancellationToken cancellationToken = default)
        {
            var query = _context.UserBlocks.AsQueryable();

            if (loadProfiles)
            {
                query = query
                    .Include(query => query.BlockerUserProfile)
                    .Include(query => query.BlockedUserProfile);
            }

            return await query.FirstOrDefaultAsync(ub =>
                ub.BlockerUserProfileId == blockerUserProfileId &&
                ub.BlockedUserProfileId == blockedUserProfileId, cancellationToken).ConfigureAwait(false);
        }
    }
}
