using Application.Common.Interfaces;
using MediatR;

namespace Application.Notifications.Commands.DeleteNotification
{
    public record DeleteNotificationCommand(int UserProfileId, Guid NotificationId) : ICommand<Unit>;
}
