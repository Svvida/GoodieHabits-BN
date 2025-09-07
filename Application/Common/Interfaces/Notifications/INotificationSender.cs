using Application.Common.Dtos;

namespace Application.Common.Interfaces.Notifications
{
    public interface INotificationSender
    {
        Task SendNotificationAsync(int userProfileId, NotificationDto notification, CancellationToken cancellationToken = default);
    }
}
