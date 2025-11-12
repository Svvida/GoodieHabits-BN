using Application.Common.Interfaces;
using MediatR;

namespace Application.UserBlocks.Commands.UnblockUser
{
    public record UnblockUserCommand(int BlockerUserProfileId, int BlockedUserProfileId) : ICommand<Unit>;
}
