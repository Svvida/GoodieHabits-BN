using Application.Common.Interfaces;
using Application.FriendInvitations.Queries.GetUserInvitations;

namespace Application.FriendInvitations.Commands.SendInvitation
{
    public record SendInvitationCommand(int SenderUserProfileId, int ReceiverUserProfileId) : ICommand<FriendInvitationDto>;
}
