using Domain.Interfaces;
using FluentValidation;

namespace Application.UserBlocks.Commands.BlockUser
{
    public class BlockUserCommandValidator : AbstractValidator<BlockUserCommand>
    {
        public BlockUserCommandValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.BlockerUserProfileId)
                .GreaterThan(0).WithMessage("Blocker user profile ID must be greater than zero.");

            RuleFor(x => x.BlockedUserProfileId)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("Blocked user profile ID must be greater than zero.")
                .NotEqual(x => x.BlockerUserProfileId).WithMessage("You cannot block yourself.")
                .MustAsync(async (command, blockedUserProfileId, cancellationToken) =>
                {
                    bool isBlockExists = await unitOfWork.UserBlocks
                        .IsUserBlockExistsByProfileIdsAsync(command.BlockerUserProfileId, blockedUserProfileId, cancellationToken)
                        .ConfigureAwait(false);
                    return !isBlockExists;
                }).WithMessage("The user is already blocked.");
        }
    }
}
