using Api.Helpers;
using Application.FriendInvitations.Commands.SendInvitation;
using Application.FriendInvitations.Queries.GetInvitationById;
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

        [HttpGet("{invitationId}")]
        public async Task<IActionResult> GetInvitationById(int invitationId, CancellationToken cancellationToken)
        {
            var query = new GetInvitationByIdQuery(
                JwtHelpers.GetCurrentUserProfileId(User),
                invitationId);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<FriendInvitationDto>> InviteUser([FromBody] SendInvitationRequest request, CancellationToken cancellationToken)
        {
            var command = new SendInvitationCommand(
                JwtHelpers.GetCurrentUserProfileId(User),
                request.ReceiverUserProfileId);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetInvitationById), new { invitationId = result.InvitationId }, result);
        }
    }
}
