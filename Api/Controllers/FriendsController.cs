using Api.Helpers;
using Application.Friendships.Commands.RemoveFriend;
using Application.Friendships.Queries.GetMyFriendsList;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/friends")]
    public class FriendsController(ISender sender) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<FriendDto>>> GetMyFriendsList(CancellationToken cancellationToken = default)
        {
            var query = new GetMyFriendsListQuery(JwtHelpers.GetCurrentUserProfileId(User));
            var friendsList = await sender.Send(query, cancellationToken).ConfigureAwait(false);
            return Ok(friendsList);
        }

        [HttpDelete("{friendUserProfileId:int}")]
        public async Task<IActionResult> RemoveFriend(int friendUserProfileId, CancellationToken cancellationToken = default)
        {
            var command = new RemoveFriendCommand(friendUserProfileId, JwtHelpers.GetCurrentUserProfileId(User));
            await sender.Send(command, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
    }
}
