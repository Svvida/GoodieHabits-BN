using Application.Common.Dtos;
using Domain.Interfaces;
using MediatR;

namespace Application.Notifications.Queries.GetAllNotifications
{
    public class GetAllNotificationsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllNotificationsQuery, IEnumerable<NotificationDto>>
    {
        public async Task<IEnumerable<NotificationDto>> Handle(GetAllNotificationsQuery request, CancellationToken cancellationToken)
        {
            var notifications = await unitOfWork.Notifications.GetAllUserNotifications(request.UserProfileId, request.OnlyUnread, cancellationToken).ConfigureAwait(false);
            return notifications.Select(n => new NotificationDto(n.Id, n.Type.ToString(), n.Title, n.Message, n.PayloadJson));
        }
    }
}
