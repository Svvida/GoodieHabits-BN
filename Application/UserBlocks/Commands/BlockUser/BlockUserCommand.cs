using Application.Common.Interfaces;
using MediatR;

namespace Application.UserBlocks.Commands.BlockUser
{
    public record BlockUserCommand(int BlockedUserProfileId, int BlockerUserProfileId) : ICommand<Unit>;
}
