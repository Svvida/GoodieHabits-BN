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
    public class SendInvitationCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IClock clock, INotificationService notificationService) : IRequestHandler<SendInvitationCommand, FriendInvitationDto>
    {
        public async Task<FriendInvitationDto> Handle(SendInvitationCommand command, CancellationToken cancellationToken)
        {
            var utcNow = clock.GetCurrentInstant().ToDateTimeUtc();

            var existingInvitation = await unitOfWork.FriendInvitations.GetFriendInvitationByUserProfileIdsAsync(command.SenderUserProfileId, command.ReceiverUserProfileId, false, cancellationToken).ConfigureAwait(false);
            if (existingInvitation != null)
            {
                // An invitation already existis. What is its state?
                if (existingInvitation.Status == FriendInvitationStatus.Pending)
                {
                    if (existingInvitation.SenderUserProfileId == command.SenderUserProfileId)
                    {
                        // Sender is trying to re-send a pending invite.
                        throw new FriendInvitationException("An invitation has already been sent to this user.");
                    }
                    else
                    {
                        // Receiver is trying to invite back Sender.
                        // This should be handled by UpdateInvitationStatusCommandHandler
                        throw new FriendInvitationException("You have a pending invitation from this user. Please accept it instead.");
                    }
                }

                if (existingInvitation.Status == FriendInvitationStatus.Rejected)
                {
                    var sevenDaysAgo = utcNow.AddDays(-7); // Simplified date calculation
                    if (existingInvitation.RespondedAt > sevenDaysAgo)
                        throw new FriendInvitationException("This user recently rejected an invitation. Please wait before sending another.");
                }

                // It's Rejected or Cancelled. Delete it to make way for the new one.
                unitOfWork.FriendInvitations.Remove(existingInvitation);

            }

            var senderProfile = await unitOfWork.UserProfiles.GetByIdAsync(command.SenderUserProfileId, cancellationToken)
                ?? throw new NotFoundException($"User Profile with ID {command.SenderUserProfileId} not found.");
            var receiverProfile = await unitOfWork.UserProfiles.GetByIdAsync(command.ReceiverUserProfileId, cancellationToken)
                ?? throw new NotFoundException($"User Profile with ID {command.ReceiverUserProfileId} not found.");

            var newInvitation = FriendInvitation.Create(command.SenderUserProfileId, command.ReceiverUserProfileId, utcNow);
            newInvitation.SetSender(senderProfile);
            newInvitation.SetReceiver(receiverProfile);
            await unitOfWork.FriendInvitations.AddAsync(newInvitation, cancellationToken);

            await notificationService.CreateAndSendAsync(
                userProfileId: command.ReceiverUserProfileId,
                type: NotificationTypeEnum.FriendRequestReceived,
                title: "New Friend Invite!",
                message: $"You have received a friend invitation from {senderProfile.Nickname}.",
                payload: "null", // Payload is unnecessary, we probably won't do anything with it so we can delete it.
                cancellationToken: cancellationToken).ConfigureAwait(false);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return mapper.Map<FriendInvitationDto>(newInvitation);
        }
    }
}
