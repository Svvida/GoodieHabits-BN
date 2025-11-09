using Domain.Enums;

namespace Application.FriendInvitations.Commands.UpdateInvitationStatus
{
    public record UpdateInvitationStatusRequest(UpdateFriendInvitationStatusEnum Status);
}
