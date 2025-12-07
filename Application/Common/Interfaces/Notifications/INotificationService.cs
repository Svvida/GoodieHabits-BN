using Domain.Enums;

namespace Application.Common.Interfaces.Notifications
{
    public interface INotificationService
    {
        Task CreateAndSendAsync<TPayload>(int userProfileId, NotificationTypeEnum type, string title, string message, TPayload payload, CancellationToken cancellationToken = default);
    }
}
