using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using NodaTime;

namespace Application.FriendInvitations.Commands.UpdateInvitationStatus.Strategies
{
    public class RejectInvitationStrategy(IClock clock) : IInvitationStatusUpdateStrategy
    {
        public UpdateFriendInvitationStatusEnum Status => UpdateFriendInvitationStatusEnum.Rejected;
        public Task ExecuteAsync(FriendInvitation invitation, int currentUserId, CancellationToken cancellationToken)
        {
            if (invitation.ReceiverUserProfileId != currentUserId)
                throw new ForbiddenException("Only the receiver can reject the invitation.");

            var now = clock.GetCurrentInstant().ToDateTimeUtc();
            invitation.SetRejected(now);

            return Task.CompletedTask;
        }
    }
}
