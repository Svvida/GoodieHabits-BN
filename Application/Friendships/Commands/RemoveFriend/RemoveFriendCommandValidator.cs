using Domain.Interfaces;
using FluentValidation;

namespace Application.Friendships.Commands.RemoveFriend
{
    public class RemoveFriendCommandValidator : AbstractValidator<RemoveFriendCommand>
    {
        public RemoveFriendCommandValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.UserProfileId)
                .GreaterThan(0).WithMessage("{PropertyName} must be greater than 0.");

            RuleFor(x => x.FriendUserProfileId)
                .GreaterThan(0).WithMessage("{PropertyName} must be greater than 0.")
                .MustAsync(async (command, friendUserProfileId, cancellationToken) =>
                {
                    return await unitOfWork.Friends.IsFriendshipExistsByUserProfileIdsAsync(command.UserProfileId, friendUserProfileId, cancellationToken);
                })
                .WithMessage("Friendship does not exist.");
        }
    }
}
