using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.FriendInvitations.Commands.UpdateInvitationStatus
{
    public class UpdateInvitationStatusCommandHandler(IUnitOfWork unitOfWork, IEnumerable<IInvitationStatusUpdateStrategy> strategies) : IRequestHandler<UpdateInvitationStatusCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateInvitationStatusCommand command, CancellationToken cancellationToken)
        {
            var invitation = await unitOfWork.FriendInvitations.GetUserInvitationByIdAsync(command.UserProfileId, command.InvitationId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Invitation with ID {command.InvitationId} not found.");

            var strategy = strategies.FirstOrDefault(s => s.Status == command.Status)
                ?? throw new FriendInvitationException($"No strategy found for status '{command.Status}.");

            await strategy.ExecuteAsync(invitation, command.UserProfileId, cancellationToken).ConfigureAwait(false);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Unit.Value;
        }
    }
}
