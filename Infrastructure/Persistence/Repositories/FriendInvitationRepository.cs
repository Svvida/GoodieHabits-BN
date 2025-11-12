using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using NodaTime;

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

            query = query.Include(fi => fi.Sender)
                         .Include(fi => fi.Receiver);

            if (direction == InvitationDirection.Sent)
            {
                query = query.Where(fi => fi.SenderUserProfileId == userProfileId);
            }
            else if (direction == InvitationDirection.Received)
            {
                query = query.Where(fi => fi.ReceiverUserProfileId == userProfileId);
            }
            else
            {
                query = query.Where(fi => fi.SenderUserProfileId == userProfileId || fi.ReceiverUserProfileId == userProfileId);
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

        public async Task<FriendshipEligibilityStatus> CheckFriendshipEligibilityAsync(int senderUserProfileId, int receiverUserProfileId, Instant nowUtc, CancellationToken cancellationToken = default)
        {
            var lowerId = Math.Min(senderUserProfileId, receiverUserProfileId);
            var higherId = Math.Max(senderUserProfileId, receiverUserProfileId);

            // Check 1: Are they already friends?
            if (await context.Friendships.AnyAsync(f => f.UserProfileId1 == lowerId && f.UserProfileId2 == higherId, cancellationToken))
                return FriendshipEligibilityStatus.AlreadyFriends;

            // Check 2: Is there an existing pending or rejected invitation between them (in either direction)?
            var existingInvitation = await context.FriendInvitations
                .Where(fi =>
                    (fi.SenderUserProfileId == senderUserProfileId && fi.ReceiverUserProfileId == receiverUserProfileId) ||
                    (fi.SenderUserProfileId == receiverUserProfileId && fi.ReceiverUserProfileId == senderUserProfileId))
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            if (existingInvitation is not null)
            {
                switch (existingInvitation.Status)
                {
                    case FriendInvitationStatus.Pending:
                        // An active, pending invitation already exists
                        return FriendshipEligibilityStatus.InvitationExists;

                    case FriendInvitationStatus.Rejected:
                        // An invitation was rejected. Check if enough time has passed.
                        var sevenDaysAgo = nowUtc.Minus(Duration.FromDays(7)).ToDateTimeUtc();
                        if (existingInvitation.RespondedAt.HasValue && existingInvitation.RespondedAt > sevenDaysAgo)
                            return FriendshipEligibilityStatus.RecentlyRejected;
                        break;
                }
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

        public async Task<FriendInvitation?> GetPendingInvitationAsync(int userProfileId1, int userProfileId2, CancellationToken cancellationToken = default)
        {
            return await context.FriendInvitations
                .AsNoTracking()
                .FirstOrDefaultAsync(fi => (fi.SenderUserProfileId == userProfileId1 && fi.ReceiverUserProfileId == userProfileId2) ||
                                 (fi.ReceiverUserProfileId == userProfileId1 && fi.SenderUserProfileId == userProfileId2) &&
                                 fi.Status == FriendInvitationStatus.Pending,
                                 cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
