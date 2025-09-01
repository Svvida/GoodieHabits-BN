using Application.Common.Dtos;
using Application.Common.Interfaces;

namespace Application.Notifications.Queries.GetAllNotifications
{
    public record GetAllNotificationsQuery(int UserProfileId, bool OnlyUnread) : IQuery<IEnumerable<NotificationDto>>;
}
