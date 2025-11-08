using Api.Helpers;
using Application.UserBlocks.Commands.BlockUser;
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
    }
}
