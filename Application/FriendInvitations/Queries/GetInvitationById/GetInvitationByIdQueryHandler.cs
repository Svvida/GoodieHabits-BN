using Application.FriendInvitations.Queries.GetUserInvitations;
using Domain.Exceptions;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.FriendInvitations.Queries.GetInvitationById
{
    public class GetInvitationByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetInvitationByIdQuery, FriendInvitationDto>
    {
        public async Task<FriendInvitationDto> Handle(GetInvitationByIdQuery query, CancellationToken cancellationToken)
        {
            var friendInvitation = await unitOfWork.FriendInvitations
                .GetUserInvitationByIdAsync(query.UserProfileId, query.InvitationId, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException("Invitation not found.");
            return mapper.Map<FriendInvitationDto>(friendInvitation);
        }
    }
}
