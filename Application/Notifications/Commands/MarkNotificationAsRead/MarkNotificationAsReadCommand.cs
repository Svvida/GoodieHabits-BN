

using Application.Common.Interfaces;

namespace Application.Notifications.Commands.MarkNotificationAsRead
{
    public record MarkNotificationAsReadCommand(Guid NotificationId, int UserProfileId) : ICommand;
}
