using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Friendships.Commands.RemoveFriend
{
    public class RemoveFriendCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<RemoveFriendCommand, Unit>
    {
        public async Task<Unit> Handle(RemoveFriendCommand command, CancellationToken cancellationToken)
        {
            var friendship = await unitOfWork.Friends.GetFriendshipByUserProfileIdsAsync(command.UserProfileId, command.FriendUserProfileId, true, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException("Friendship not found.");

            friendship.UserProfile1.DecreaseFriendsCount();
            friendship.UserProfile2.DecreaseFriendsCount();

            unitOfWork.Friends.Remove(friendship);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
