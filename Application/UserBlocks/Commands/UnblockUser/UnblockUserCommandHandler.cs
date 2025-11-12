using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.UserBlocks.Commands.UnblockUser
{
    public class UnblockUserCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UnblockUserCommand, Unit>
    {
        public async Task<Unit> Handle(UnblockUserCommand command, CancellationToken cancellationToken)
        {
            var userBlock = await unitOfWork.UserBlocks.GetUserBlockByProfileIdsAsync(
                command.BlockerUserProfileId,
                command.BlockedUserProfileId,
                loadProfiles: false,
                cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException("UserBlock not found.");

            unitOfWork.UserBlocks.Remove(userBlock);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Unit.Value;
        }
    }
}
