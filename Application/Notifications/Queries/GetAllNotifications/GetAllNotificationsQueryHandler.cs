using Application.Common.Dtos;
using Domain.Interfaces;
using Mapster;
using MediatR;

namespace Application.Notifications.Queries.GetAllNotifications
{
    public class GetAllNotificationsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllNotificationsQuery, IEnumerable<NotificationDto>>
    {
        public async Task<IEnumerable<NotificationDto>> Handle(GetAllNotificationsQuery request, CancellationToken cancellationToken)
        {
            var notifications = await unitOfWork.Notifications.GetAllUserNotifications(request.UserProfileId, request.OnlyUnread, cancellationToken).ConfigureAwait(false);
            return notifications.Adapt<IEnumerable<NotificationDto>>();
        }
    }
}
