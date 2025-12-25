using Api.Helpers;
using Application.Common.Dtos;
using Application.Notifications.Commands.DeleteNotification;
using Application.Notifications.Commands.MarkAllNotificationsAsRead;
using Application.Notifications.Commands.MarkNotificationAsRead;
using Application.Notifications.Queries.GetAllNotifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/notifications")]
    public class NotificationController(ISender sender) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetAllNotificationsAsync([FromQuery] bool onlyUnread = false, CancellationToken cancellationToken = default)
        {
            var query = new GetAllNotificationsQuery(JwtHelpers.GetCurrentUserProfileId(User), onlyUnread);
            var notifications = await sender.Send(query, cancellationToken).ConfigureAwait(false);
            return Ok(notifications);
        }

        [HttpPatch("{id:guid}/mark-read")]
        public async Task<IActionResult> MarkNotificationAsReadAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var command = new MarkNotificationAsReadCommand(id, JwtHelpers.GetCurrentUserProfileId(User));

            await sender.Send(command, cancellationToken).ConfigureAwait(false);

            return NoContent();
        }

        [HttpPatch("mark-all-read")]
        public async Task<IActionResult> MarkAllNotificationsAsReadAsync(CancellationToken cancellationToken = default)
        {
            var command = new MarkAllNotificationsAsReadCommand(JwtHelpers.GetCurrentUserProfileId(User));
            await sender.Send(command, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteNotificationAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var command = new DeleteNotificationCommand(JwtHelpers.GetCurrentUserProfileId(User), id);
            await sender.Send(command, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
    }
}
