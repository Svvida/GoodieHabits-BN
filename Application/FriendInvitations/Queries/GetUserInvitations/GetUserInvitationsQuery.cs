using Application.Common.Interfaces;
using Domain.Enums;

namespace Application.FriendInvitations.Queries.GetUserInvitations
{
    public record GetUserInvitationsQuery(int UserProfileId, InvitationDirection? Direction) : IQuery<List<FriendInvitationDto>>;
}
