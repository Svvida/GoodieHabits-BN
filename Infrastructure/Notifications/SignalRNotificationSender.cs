using Application.Common.Dtos;
using Application.Common.Interfaces.Notifications;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Notifications
{
    public class SignalRNotificationSender(IHubContext<NotificationHub> hubContext, ILogger<INotificationSender> logger) : INotificationSender
    {
        public async Task SendNotificationAsync(int userProfileId, NotificationDto notification)
        {
            try
            {
                await hubContext.Clients.User(userProfileId.ToString()).SendAsync("ReceiveNotification", notification);
                logger.LogInformation($"Notification sent to userProfileId: {userProfileId}, NotificationId: {notification.Id}.");
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Failed to send notification to userProfileId: {userProfileId}, NotificationId: {notification.Id}, Exception: {ex.Message}.");
            }
        }
    }
}
