using Application.Common.Dtos;
using Domain.Interfaces;
using MediatR;
using NodaTime;

namespace Application.Notifications.Queries.GetAllNotifications
{
    public class GetAllNotificationsQueryHandler(IUnitOfWork unitOfWork, IClock clock) : IRequestHandler<GetAllNotificationsQuery, IEnumerable<NotificationDto>>
    {
        public async Task<IEnumerable<NotificationDto>> Handle(GetAllNotificationsQuery request, CancellationToken cancellationToken)
        {
            var notifications = await unitOfWork.Notifications.GetAllUserNotifications(request.UserProfileId, request.OnlyUnread, cancellationToken).ConfigureAwait(false);
            return notifications.Select(n => new NotificationDto(n.Id, n.Type.ToString(), n.IsRead, n.Title, n.Message, n.PayloadJson, clock.GetCurrentInstant().ToDateTimeUtc()));
        }
    }
}
