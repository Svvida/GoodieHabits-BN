using Domain.Enums;
using Domain.Models;

namespace Application.FriendInvitations.Commands.UpdateInvitationStatus
{
    public interface IInvitationStatusUpdateStrategy
    {
        UpdateFriendInvitationStatusEnum Status { get; }
        Task ExecuteAsync(FriendInvitation invitation, int currentUserId, CancellationToken cancellationToken);
    }
}
