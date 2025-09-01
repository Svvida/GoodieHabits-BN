using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Notifications.Commands.MarkNotificationAsRead
{
    public class MarkNotficationAsReadCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<MarkNotificationAsReadCommand, Unit>
    {
        public async Task<Unit> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
        {
            var notification = await unitOfWork.Notifications.GetUserNotificationByIdAsync(request.NotificationId, request.UserProfileId, false, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Notification with ID {request.NotificationId} not found for user {request.UserProfileId}.");

            if (!notification.IsRead)
            {
                notification.MarkAsRead();
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            return Unit.Value;
        }
    }
}
