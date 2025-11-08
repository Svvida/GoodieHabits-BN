using Api.Helpers;
using Application.FriendInvitations.Queries.GetUserInvitations;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/friend-invitations")]
    [Authorize]
    public class FriendInvitationController(ISender sender) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetUserInvitations([FromQuery] InvitationDirection? direction, CancellationToken cancellationToken)
        {
            var query = new GetUserInvitationsQuery(
                JwtHelpers.GetCurrentUserProfileId(User),
                direction);

            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return Ok(result);
        }
    }
}
