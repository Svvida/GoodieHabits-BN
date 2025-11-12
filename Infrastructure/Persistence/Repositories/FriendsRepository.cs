using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class FriendsRepository(AppDbContext context) : BaseRepository<Friendship>(context), IFriendsRepository
    {
        public async Task<List<Friendship>> GetUserFriendsAsync(int userProfileId, CancellationToken cancellationToken = default)
        {
            return await context.Friendships
                .Where(f => f.UserProfileId1 == userProfileId || f.UserProfileId2 == userProfileId)
                .Include(f => f.UserProfile1)
                .Include(f => f.UserProfile2)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<Friendship?> GetFriendshipByUserProfileIdsAsync(int userProfileId1, int userProfileId2, bool loadProfiles = false, CancellationToken cancellationToken = default)
        {
            var lowerId = Math.Min(userProfileId1, userProfileId2);
            var higherId = Math.Max(userProfileId1, userProfileId2);

            var query = context.Friendships.AsQueryable();

            if (loadProfiles)
            {
                query = query
                    .Include(f => f.UserProfile1)
                    .Include(f => f.UserProfile2);
            }

            return await query.FirstOrDefaultAsync(f => f.UserProfileId1 == lowerId && f.UserProfileId2 == higherId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<bool> IsFriendshipExistsByUserProfileIdsAsync(int userProfileId1, int userProfileId2, CancellationToken cancellationToken = default)
        {
            var lowerId = Math.Min(userProfileId1, userProfileId2);
            var higherId = Math.Max(userProfileId1, userProfileId2);

            return await context.Friendships
                .AnyAsync(f => f.UserProfileId1 == lowerId && f.UserProfileId2 == higherId, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
