using Application.Common.Dtos;
using Application.Common.Interfaces.Notifications;
using Application.FriendInvitations.Queries.GetUserInvitations;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MapsterMapper;
using MediatR;
using NodaTime;

namespace Application.FriendInvitations.Commands.SendInvitation
{
    public class SendInvitationCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IClock clock, INotificationSender notificationSender) : IRequestHandler<SendInvitationCommand, FriendInvitationDto>
    {
        public async Task<FriendInvitationDto> Handle(SendInvitationCommand command, CancellationToken cancellationToken)
        {
            var senderProfile = await unitOfWork.UserProfiles.GetByIdAsync(command.SenderUserProfileId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"User Profile with ID {command.SenderUserProfileId} not found.");
            var receiverProfile = await unitOfWork.UserProfiles.GetByIdAsync(command.ReceiverUserProfileId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"User Profile with ID {command.ReceiverUserProfileId} not found.");

            var utcNow = clock.GetCurrentInstant().ToDateTimeUtc();

            var friendInvitation = FriendInvitation.Create(command.SenderUserProfileId, command.ReceiverUserProfileId, utcNow);
            await unitOfWork.FriendInvitations.AddAsync(friendInvitation, cancellationToken).ConfigureAwait(false);

            var receiverNotificationTitle = "New Friend Invite!";
            var receiverNotificationMessage = $"You have received a friend invitation from {senderProfile.Nickname}.";

            var notification = Notification.Create(
                id: Guid.NewGuid(),
                userProfileId: friendInvitation.ReceiverUserProfileId,
                type: NotificationTypeEnum.FriendRequestReceived,
                title: receiverNotificationTitle,
                message: receiverNotificationMessage,
                payloadJson: "null", // Payload is unnecessary, we probably won't do anything with it so we can delete it.
                utcNow: utcNow);

            await unitOfWork.Notifications.AddAsync(notification, cancellationToken).ConfigureAwait(false);

            var notificationDto = new NotificationDto(
                Id: notification.Id,
                Type: notification.Type.ToString(),
                IsRead: notification.IsRead,
                Title: notification.Title,
                Message: notification.Message,
                Data: notification.PayloadJson,
                CreatedAt: utcNow);

            await notificationSender.SendNotificationAsync(notification.UserProfileId, notificationDto, cancellationToken).ConfigureAwait(false);

            friendInvitation.SetSender(senderProfile);
            friendInvitation.SetReceiver(receiverProfile);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return mapper.Map<FriendInvitationDto>(friendInvitation);
        }
    }
}
