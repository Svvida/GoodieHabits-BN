using Application.Common.Dtos;
using Application.Common.Interfaces.Notifications;
using Domain.Enums;
using Domain.Interfaces;
using Mapster;
using NodaTime;

namespace Infrastructure.Notifications
{
    public class NotificationService(IUnitOfWork unitOfWork, INotificationSender notificationSender, IClock clock) : INotificationService
    {
        public async Task CreateAndSendAsync<TPayload>(int userProfileId, NotificationTypeEnum type, string title, string message, TPayload payload, CancellationToken cancellationToken)
        {
            var utcNow = clock.GetCurrentInstant().ToDateTimeUtc();
            var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);

            var notification = Domain.Models.Notification.Create(
                id: Guid.NewGuid(),
                userProfileId: userProfileId,
                type: type,
                title: title,
                message: message,
                payloadJson: payloadJson,
                utcNow: utcNow);

            await unitOfWork.Notifications.AddAsync(notification, cancellationToken).ConfigureAwait(false);

            var notificationDto = notification.Adapt<NotificationDto>();

            await notificationSender.SendNotificationAsync(userProfileId, notificationDto, cancellationToken).ConfigureAwait(false);
        }
    }
}
