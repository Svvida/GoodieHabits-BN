using Domain.Interfaces;
using Domain.Models;
using MediatR;
using NodaTime;

namespace Application.UserBlocks.Commands.BlockUser
{
    public class BlockUserCommandHandler(IUnitOfWork unitOfWork, IClock clock) : IRequestHandler<BlockUserCommand, Unit>
    {
        public async Task<Unit> Handle(BlockUserCommand command, CancellationToken cancellationToken)
        {
            var friendship = await unitOfWork.Friends.GetFriendshipByUserProfileIdsAsync(command.BlockerUserProfileId, command.BlockedUserProfileId, true, cancellationToken).ConfigureAwait(false);

            if (friendship is not null)
            {
                friendship.UserProfile1.DecreaseFriendsCount();
                friendship.UserProfile2.DecreaseFriendsCount();
                unitOfWork.Friends.Remove(friendship);
            }

            var friendInvitation = await unitOfWork.FriendInvitations.GetFriendInvitationByUserProfileIdsAsync(command.BlockerUserProfileId, command.BlockedUserProfileId, false, cancellationToken).ConfigureAwait(false);

            if (friendInvitation is not null)
            {
                unitOfWork.FriendInvitations.Remove(friendInvitation);
            }

            var userBlock = UserBlock.Create(command.BlockerUserProfileId, command.BlockedUserProfileId, clock.GetCurrentInstant().ToDateTimeUtc());
            await unitOfWork.UserBlocks.AddAsync(userBlock, cancellationToken).ConfigureAwait(false);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Unit.Value;
        }
    }
}
