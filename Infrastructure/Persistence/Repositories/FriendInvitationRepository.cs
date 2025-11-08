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

        public async Task<FriendInvitation?> GetUserInvitationByIdAsync(int userProfileId, int invitationId, CancellationToken cancellationToken = default)
        {
            return await context.FriendInvitations
                .Include(fi => fi.Sender)
                .Include(fi => fi.Receiver)
                .FirstOrDefaultAsync(fi => fi.Id == invitationId &&
                                           (fi.SenderUserProfileId == userProfileId || fi.ReceiverUserProfileId == userProfileId),
                                           cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<bool> IsUserInvitationExistByIdAsync(int userProfileId, int invitationId, CancellationToken cancellationToken = default)
        {
            return await context.FriendInvitations
                .AnyAsync(fi => fi.Id == invitationId &&
                                (fi.SenderUserProfileId == userProfileId || fi.ReceiverUserProfileId == userProfileId),
                                cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<FriendshipEligibilityStatus> CheckFriendshipEligibilityAsync(int senderUserProfileId, int receiverUserProfileId, CancellationToken cancellationToken = default)
        {
            var lowerId = Math.Min(senderUserProfileId, receiverUserProfileId);
            var higherId = Math.Max(senderUserProfileId, receiverUserProfileId);

            // Check 1: Are they already friends?
            if (await context.Friendships.AnyAsync(f => f.UserProfileId1 == lowerId && f.UserProfileId2 == higherId, cancellationToken))
                return FriendshipEligibilityStatus.AlreadyFriends;

            // Check 2: Is there an existing pending invitation between them (in either direction)?
            if (await context.FriendInvitations.AnyAsync(fi =>
                fi.Status == FriendInvitationStatus.Pending &&
                ((fi.SenderUserProfileId == senderUserProfileId && fi.ReceiverUserProfileId == receiverUserProfileId) ||
                  (fi.SenderUserProfileId == receiverUserProfileId && fi.ReceiverUserProfileId == senderUserProfileId)), cancellationToken))
            {
                return FriendshipEligibilityStatus.InvitationExists;
            }

            // Check 3: Has the receiver blocked the sender?
            if (await context.UserBlocks.AnyAsync(ub => ub.BlockerUserProfileId == receiverUserProfileId && ub.BlockedUserProfileId == senderUserProfileId, cancellationToken))
            {
                return FriendshipEligibilityStatus.BlockedByRecipient;
            }

            // Check 4: Has the sender blocked the receiver?
            if (await context.UserBlocks.AnyAsync(ub => ub.BlockerUserProfileId == senderUserProfileId && ub.BlockedUserProfileId == receiverUserProfileId, cancellationToken))
            {
                return FriendshipEligibilityStatus.SenderIsBlocking;
            }

            // If all checks pass:
            return FriendshipEligibilityStatus.Eligible;
        }
    }
}
