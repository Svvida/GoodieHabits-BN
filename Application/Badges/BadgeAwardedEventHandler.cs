using Application.Common;
using Application.Common.Interfaces.Notifications;
using Domain.Enums;
using Domain.Events.Badges;
using MediatR;

namespace Application.Badges
{
    public class BadgeAwardedEventHandler(INotificationService notificationService) : INotificationHandler<DomainEventNotification<BadgeAwardedEvent>>
    {
        public async Task Handle(DomainEventNotification<BadgeAwardedEvent> wrappedNotification, CancellationToken cancellationToken)
        {
            var userProfileId = wrappedNotification.DomainEvent.UserProfileId;
            var badge = wrappedNotification.DomainEvent.Badge;

            var payloadData = new
            {
                badgeId = badge.Id,
                badgeType = badge.Type.ToString(),
                badgeText = badge.Text,
                badgeDescription = badge.Description,
                color = badge.ColorHex
            };

            await notificationService.CreateAndSendAsync(
                userProfileId: userProfileId,
                type: NotificationTypeEnum.BadgeEarned,
                title: "New Badge Earned!",
                message: $"Congratulations! You've earned the '{badge.Text}' badge.",
                payload: payloadData,
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
