using Domain.Enums;
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

        public async Task<List<FriendInvitation>> GetUserInvitationsAsync(int userProfileId, InvitationDirection? direction, CancellationToken cancellationToken = default)
        {
            var query = context.FriendInvitations.AsQueryable();

            if (direction == InvitationDirection.Sent)
            {
                query = query.Where(fi => fi.SenderUserProfileId == userProfileId)
                             .Include(fi => fi.Sender)
                             .Include(fi => fi.Receiver);
            }
            else if (direction == InvitationDirection.Received)
            {
                query = query.Where(fi => fi.ReceiverUserProfileId == userProfileId)
                             .Include(fi => fi.Sender)
                             .Include(fi => fi.Receiver);
            }
            else
            {
                query = query.Where(fi => fi.SenderUserProfileId == userProfileId || fi.ReceiverUserProfileId == userProfileId)
                             .Include(fi => fi.Sender)
                             .Include(fi => fi.Receiver);
            }

            return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> IsFriendInvitationExistByProfileIdsAsync(int userProfileId1, int userProfileId2, CancellationToken cancellationToken = default)
        {
            return await context.FriendInvitations
                .AnyAsync(fi => (fi.SenderUserProfileId == userProfileId1 && fi.ReceiverUserProfileId == userProfileId2) ||
                                 (fi.ReceiverUserProfileId == userProfileId1 && fi.SenderUserProfileId == userProfileId2),
                                 cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
