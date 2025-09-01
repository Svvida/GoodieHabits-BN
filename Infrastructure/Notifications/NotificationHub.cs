using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Notifications
{
    [Authorize]
    public class NotificationHub(ILogger<NotificationHub> logger) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            logger.LogInformation($"Client connected: {Context.ConnectionId}, User: {Context.UserIdentifier}.");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            logger.LogInformation($"Client: {Context.ConnectionId}, User: {Context.UserIdentifier}, Exception: {exception?.Message}.");

            await base.OnDisconnectedAsync(exception);
        }
    }
}
