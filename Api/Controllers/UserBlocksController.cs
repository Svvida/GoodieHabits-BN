using Api.Helpers;
using Application.UserBlocks.Commands.BlockUser;
using Application.UserBlocks.Commands.UnblockUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/user-blocks")]
    [Authorize]
    public class UserBlocksController(ISender sender) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> BlockUser([FromBody] BlockUserRequest request, CancellationToken cancellationToken)
        {
            var command = new BlockUserCommand(
                BlockedUserProfileId: request.BlockedUserProfileId,
                BlockerUserProfileId: JwtHelpers.GetCurrentUserProfileId(User)
            );
            await sender.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{blockedUserProfileId}")]
        public async Task<IActionResult> UnblockUser(int blockedUserProfileId, CancellationToken cancellationToken)
        {
            var command = new UnblockUserCommand(
                BlockedUserProfileId: blockedUserProfileId,
                BlockerUserProfileId: JwtHelpers.GetCurrentUserProfileId(User)
            );
            await sender.Send(command, cancellationToken);
            return NoContent();
        }
    }
}
