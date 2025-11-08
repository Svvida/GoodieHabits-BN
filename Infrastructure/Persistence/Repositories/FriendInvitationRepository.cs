using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class FriendInvitationRepository(AppDbContext context) : BaseRepository<FriendInvitation>(context), IFriendInvitationRepository
    {
        public async Task<FriendInvitation?> GetFriendInvitationByUserProfileIdsAsync(int userProfileId1, int userProfileId2, bool loadProfiles, CancellationToken cancellationToken = default)
        {
            var query = context.FriendInvitations.AsQueryable();
            if (loadProfiles)
            {
                query = query
                    .Include(fi => fi.Sender)
                    .Include(fi => fi.Receiver);
            }

            return await query.FirstOrDefaultAsync(fi =>
                (fi.SenderUserProfileId == userProfileId1 && fi.ReceiverUserProfileId == userProfileId2) ||
                (fi.SenderUserProfileId == userProfileId2 && fi.ReceiverUserProfileId == userProfileId1), cancellationToken).ConfigureAwait(false);
        }
    }
}
