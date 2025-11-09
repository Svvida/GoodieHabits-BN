using Application.Common.Interfaces;
using Application.FriendInvitations.Queries.GetUserInvitations;

namespace Application.FriendInvitations.Queries.GetInvitationById
{
    public record GetInvitationByIdQuery(int UserProfileId, int InvitationId) : IQuery<FriendInvitationDto>;
}
