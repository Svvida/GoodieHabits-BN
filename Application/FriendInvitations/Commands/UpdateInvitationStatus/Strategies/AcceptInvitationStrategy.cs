using Application.Common.Dtos;
using Application.Common.Interfaces.Notifications;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using NodaTime;

namespace Application.FriendInvitations.Commands.UpdateInvitationStatus.Strategies
{
    public class AcceptInvitationStrategy(IUnitOfWork unitOfWork, INotificationSender notificationSender, IClock clock) : IInvitationStatusUpdateStrategy
    {
        public UpdateFriendInvitationStatusEnum Status => UpdateFriendInvitationStatusEnum.Accepted;

        public async Task ExecuteAsync(FriendInvitation invitation, int currentUserId, CancellationToken cancellationToken)
        {
            if (invitation.ReceiverUserProfileId != currentUserId)
                throw new ForbiddenException("Only the receiver can accept the invitation.");

            var utcNow = clock.GetCurrentInstant().ToDateTimeUtc();
            invitation.SetAccepted(utcNow);

            var friendship = Friendship.Create(invitation.SenderUserProfileId, invitation.ReceiverUserProfileId, utcNow);
            await unitOfWork.Friends.AddAsync(friendship, cancellationToken).ConfigureAwait(false);

            invitation.Sender.IncreaseFriendsCount();
            invitation.Receiver.IncreaseFriendsCount();

            // Send notification to the sender about acceptance
            var notification = Notification.Create(
                Guid.NewGuid(),
                invitation.SenderUserProfileId,
                NotificationTypeEnum.FriendRequestAccepted,
                "You have a new friend!",
                $"{invitation.Receiver.Nickname} accepted your friend invitation!",
                "null",
                utcNow);

            await unitOfWork.Notifications.AddAsync(notification, cancellationToken).ConfigureAwait(false);

            var notificationDto = new NotificationDto(
                notification.Id,
                notification.Type.ToString(),
                notification.IsRead,
                notification.Title,
                notification.Message,
                notification.PayloadJson,
                utcNow);

            await notificationSender.SendNotificationAsync(invitation.SenderUserProfileId, notificationDto, cancellationToken).ConfigureAwait(false);

            unitOfWork.FriendInvitations.Remove(invitation);
        }
    }
}
