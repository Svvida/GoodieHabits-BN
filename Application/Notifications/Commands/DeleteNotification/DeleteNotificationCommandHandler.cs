using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Notifications.Commands.DeleteNotification
{
    public class DeleteNotificationCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteNotificationCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
        {
            var notification = await unitOfWork.Notifications.GetUserNotificationByIdAsync(request.NotificationId, request.UserProfileId, false, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException("User notification not found.");

            unitOfWork.Notifications.Remove(notification);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Unit.Value;
        }
    }
}
