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
            return await context.UserBlocks.AnyAsync(ub =>
                ub.BlockerUserProfileId == blockerUserProfileId &&
                ub.BlockedUserProfileId == blockedUserProfileId, cancellationToken).ConfigureAwait(false);
        }
    }
}
