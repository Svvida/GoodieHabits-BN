using Domain.Interfaces;
using MediatR;

namespace Application.Notifications.Commands.MarkAllNotificationsAsRead
{
    public class MarkAllNotificationsAsReadCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<MarkAllNotificationsAsReadCommand, Unit>
    {
        public async Task<Unit> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
        {
            await unitOfWork.Notifications.MarkAllUserNotificationsAsReadAsync(request.UserProfileId, cancellationToken).ConfigureAwait(false);
            return Unit.Value;
        }
    }
}
