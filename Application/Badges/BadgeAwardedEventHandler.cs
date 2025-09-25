using System.Text.Json;
using Application.Common;
using Application.Common.Dtos;
using Application.Common.Interfaces.Notifications;
using Domain.Enums;
using Domain.Events.Badges;
using Domain.Interfaces;
using Domain.Models;
using MediatR;

namespace Application.Badges
{
    public class BadgeAwardedEventHandler(IUnitOfWork unitOfWork, INotificationSender notificationSender) : INotificationHandler<DomainEventNotification<BadgeAwardedEvent>>
    {
        public async Task Handle(DomainEventNotification<BadgeAwardedEvent> wrappedNotification, CancellationToken cancellationToken)
        {
            var userProfileId = wrappedNotification.DomainEvent.UserProfileId;
            var badge = wrappedNotification.DomainEvent.Badge;

            var title = "New Badge Earned!";
            var message = $"Congratulations! You've earned the '{badge.Text}' badge.";

            var payloadData = new
            {
                badgeId = badge.Id,
                badgeType = badge.Type.ToString(),
                badgeText = badge.Text,
                badgeDescription = badge.Description,
                color = badge.ColorHex
            };
            var payloadJson = JsonSerializer.Serialize(payloadData);

            var notification = Notification.Create(
                id: Guid.NewGuid(),
                userProfileId: userProfileId,
                NotificationTypeEnum.BadgeEarned,
                title: title,
                message: message,
                payloadJson: payloadJson);

            await unitOfWork.Notifications.AddAsync(notification, cancellationToken).ConfigureAwait(false);

            var notificationDto = new NotificationDto(
                Id: notification.Id,
                Type: notification.Type.ToString(),
                Title: notification.Title,
                Message: notification.Message,
                Data: notification.PayloadJson);

            await notificationSender.SendNotificationAsync(userProfileId, notificationDto, cancellationToken).ConfigureAwait(false);
        }
    }
}
