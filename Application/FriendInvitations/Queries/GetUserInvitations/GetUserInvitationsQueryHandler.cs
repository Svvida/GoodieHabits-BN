using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.FriendInvitations.Queries.GetUserInvitations
{
    public class GetUserInvitationsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetUserInvitationsQuery, List<FriendInvitationDto>>
    {
        public async Task<List<FriendInvitationDto>> Handle(GetUserInvitationsQuery query, CancellationToken cancellationToken)
        {
            var invitations = await unitOfWork.FriendInvitations.GetUserInvitationsAsync(query.UserProfileId, query.Direction, cancellationToken).ConfigureAwait(false);

            return mapper.Map<List<FriendInvitationDto>>(invitations);
        }
    }
}
