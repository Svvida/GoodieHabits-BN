using Application.Common.Interfaces;
using MediatR;

namespace Application.Friendships.Commands.RemoveFriend
{
    public record RemoveFriendCommand(int FriendUserProfileId, int UserProfileId) : ICommand<Unit>;
}
