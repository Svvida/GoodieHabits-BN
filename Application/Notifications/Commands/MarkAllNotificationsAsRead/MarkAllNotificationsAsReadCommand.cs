using Application.Common.Interfaces;

namespace Application.Notifications.Commands.MarkAllNotificationsAsRead
{
    public record MarkAllNotificationsAsReadCommand(int UserProfileId) : ICommand;
}
