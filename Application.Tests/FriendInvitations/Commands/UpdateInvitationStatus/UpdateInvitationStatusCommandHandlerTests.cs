using Application.Common.Dtos;
using Application.Common.Interfaces.Notifications;
using Application.FriendInvitations.Commands.UpdateInvitationStatus;
using Application.FriendInvitations.Commands.UpdateInvitationStatus.Strategies;
using Domain.Enums;
using Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace Application.Tests.FriendInvitations.Commands.UpdateInvitationStatus
{
    public class UpdateInvitationStatusCommandHandlerTests : TestBase<UpdateInvitationStatusCommandHandler>
    {
        private readonly UpdateInvitationStatusCommandHandler _handler;
        private readonly Mock<INotificationSender> _notificationSenderMock;

        public UpdateInvitationStatusCommandHandlerTests() : base()
        {
            _notificationSenderMock = new Mock<INotificationSender>();

            var strategies = new List<IInvitationStatusUpdateStrategy>
            {
                new AcceptInvitationStrategy(_unitOfWork, _notificationSenderMock.Object, _clockMock.Object),
                new RejectInvitationStrategy(_clockMock.Object),
                new CancelInvitationStrategy()
            };

            _handler = new UpdateInvitationStatusCommandHandler(_unitOfWork, strategies);
        }

        [Fact]
        public async Task Handle_AcceptInvitation_ShouldAcceptInvitationAndSendNotification()
        {
            // Arrange
            var sender = await AddAccountAsync("sender@example.com", "hashedpass", "senderNick");
            var receiver = await AddAccountAsync("receiver@example.com", "hashedpass", "receiverNick");
            var invitation = await AddFriendInvitationAsync(sender.Profile.Id, receiver.Profile.Id);

            var command = new UpdateInvitationStatusCommand(
                InvitationId: invitation.Id,
                UserProfileId: receiver.Profile.Id,
                Status: UpdateFriendInvitationStatusEnum.Accepted);

            NotificationDto? capturedNotificationDto = null;

            _notificationSenderMock
                .Setup(ns => ns.SendNotificationAsync(
                    It.IsAny<int>(),
                    It.IsAny<NotificationDto>(),
                    It.IsAny<CancellationToken>()))
                .Callback<int, NotificationDto, CancellationToken>((userId, dto, ct) =>
                {
                    capturedNotificationDto = dto;
                })
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(1, sender.Profile.FriendsCount);
            Assert.Equal(1, receiver.Profile.FriendsCount);

            var updatedInvitation = await _unitOfWork.FriendInvitations.GetByIdAsync(invitation.Id, CancellationToken.None);
            updatedInvitation.Should().BeNull(); // Invitation should be removed after acceptance

            var friendship = await _unitOfWork.Friends.GetFriendshipByUserProfileIdsAsync(sender.Profile.Id, receiver.Profile.Id, false, CancellationToken.None);
            friendship.Should().NotBeNull();

            Assert.NotNull(capturedNotificationDto);
            var notificationInDb = await _unitOfWork.Notifications.GetUserNotificationByIdAsync(capturedNotificationDto.Id, sender.Profile.Id, true, CancellationToken.None);
            notificationInDb.Should().NotBeNull();
            notificationInDb!.UserProfileId.Should().Be(sender.Profile.Id);

            _notificationSenderMock.Verify(ns => ns.SendNotificationAsync(
                sender.Profile.Id,
                It.IsAny<NotificationDto>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_RejectInvitation_ShouldRejectInvitation()
        {
            // Arrange
            var sender = await AddAccountAsync("sender@example.com", "hashedpass", "senderNick");
            var receiver = await AddAccountAsync("receiver@example.com", "hashedpass", "receiverNick");
            var invitation = await AddFriendInvitationAsync(sender.Profile.Id, receiver.Profile.Id);

            var command = new UpdateInvitationStatusCommand(
                InvitationId: invitation.Id,
                UserProfileId: receiver.Profile.Id,
                Status: UpdateFriendInvitationStatusEnum.Rejected);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            var updatedInvitation = await _unitOfWork.FriendInvitations.GetByIdAsync(invitation.Id, CancellationToken.None);
            updatedInvitation.Should().NotBeNull();
            updatedInvitation!.Status.Should().Be(FriendInvitationStatus.Rejected);
        }

        [Fact]
        public async Task Handle_CancelInvitation_ShouldCancelInvitation()
        {
            // Arrange
            var sender = await AddAccountAsync("sender@example.com", "hashedpass", "senderNick");
            var receiver = await AddAccountAsync("receiver@example.com", "hashedpass", "receiverNick");
            var invitation = await AddFriendInvitationAsync(sender.Profile.Id, receiver.Profile.Id);

            var command = new UpdateInvitationStatusCommand(
                InvitationId: invitation.Id,
                UserProfileId: sender.Profile.Id,
                Status: UpdateFriendInvitationStatusEnum.Cancelled);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            var updatedInvitation = await _unitOfWork.FriendInvitations.GetByIdAsync(invitation.Id, CancellationToken.None);
            updatedInvitation.Should().NotBeNull();
            updatedInvitation!.Status.Should().Be(FriendInvitationStatus.Cancelled);
        }

        [Fact]
        public async Task Handle_InvalidStatus_ShouldThrowException()
        {
            // Arrange
            var sender = await AddAccountAsync("sender@example.com", "hashedpass", "senderNick");
            var receiver = await AddAccountAsync("receiver@example.com", "hashedpass", "receiverNick");
            var invitation = await AddFriendInvitationAsync(sender.Profile.Id, receiver.Profile.Id);

            var command = new UpdateInvitationStatusCommand(
                InvitationId: invitation.Id,
                UserProfileId: receiver.Profile.Id,
                Status: (UpdateFriendInvitationStatusEnum)999); // Invalid status

            // Act & Assert
            await Assert.ThrowsAsync<FriendInvitationException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_NonExistentInvitation_ShouldThrowNotFoundException()
        {
            // Arrange
            var receiver = await AddAccountAsync("receiver@example.com", "hashedpass", "receiverNick");

            var command = new UpdateInvitationStatusCommand(
                InvitationId: 999, // Non-existent invitation ID
                UserProfileId: receiver.Profile.Id,
                Status: UpdateFriendInvitationStatusEnum.Accepted);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}
