

using Application.Common.Interfaces;

namespace Application.Notifications.Commands.MarkNotificationAsRead
{
    public record MarkNotificationAsReadCommand(int NotificationId, int UserProfileId) : ICommand;
}
