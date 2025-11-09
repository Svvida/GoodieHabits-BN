using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;

namespace Application.FriendInvitations.Commands.UpdateInvitationStatus
{
    public record UpdateInvitationStatusCommand(int InvitationId, int UserProfileId, UpdateFriendInvitationStatusEnum Status) : ICommand<Unit>;
}
