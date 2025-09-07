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
            logger.LogDebug("--- SIGNALR DEBUG --- Connection ID: {ConnectionId}", Context.ConnectionId);
            logger.LogDebug("--- SIGNALR DEBUG --- User Identifier: {UserIdentifier}", Context.UserIdentifier);

            logger.LogDebug("--- SIGNALR DEBUG --- Client successfully connected");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            logger.LogDebug("--- SIGNALR DEBUG --- Connection ID: {ConnectionId}", Context.ConnectionId);
            logger.LogDebug("--- SIGNALR DEBUG --- User Identifier: {UserIdentifier}", Context.UserIdentifier);
            logger.LogDebug("--- SIGNALR DEBUG --- Exception: {Exception}", exception?.Message);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendTestMessage(string message)
        {
            logger.LogInformation("--- SIGNALR DEBUG --- SendTestMessage called by user: {UserIdentifier}", Context.UserIdentifier);
            await Clients.Caller.SendAsync("ReceiveTestMessage", $"Echo: {message}");
        }
    }
}
