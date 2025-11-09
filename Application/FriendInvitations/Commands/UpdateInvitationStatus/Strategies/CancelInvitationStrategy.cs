using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;

namespace Application.FriendInvitations.Commands.UpdateInvitationStatus.Strategies
{
    public class CancelInvitationStrategy : IInvitationStatusUpdateStrategy
    {
        public UpdateFriendInvitationStatusEnum Status => UpdateFriendInvitationStatusEnum.Cancelled;
        public Task ExecuteAsync(FriendInvitation invitation, int currentUserId, CancellationToken cancellationToken)
        {
            if (invitation.SenderUserProfileId != currentUserId)
                throw new ForbiddenException("Only the sender can cancel the invitation.");
            invitation.SetCancelled();
            return Task.CompletedTask;
        }
    }
}
