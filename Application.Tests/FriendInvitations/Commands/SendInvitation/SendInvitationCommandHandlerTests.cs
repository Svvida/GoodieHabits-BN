using Application.Common.Interfaces.Notifications;
using Application.FriendInvitations.Commands.SendInvitation;
using Domain.Enums;
using Domain.Exceptions;
using Moq;

namespace Application.Tests.FriendInvitations.Commands.SendInvitation
{
    public class SendInvitationCommandHandlerTests : TestBase<SendInvitationCommandHandler>
    {
        private readonly SendInvitationCommandHandler _handler;
        private readonly Mock<INotificationService> _notificationServiceMock;

        public SendInvitationCommandHandlerTests()
        {
            _notificationServiceMock = new Mock<INotificationService>();
            _handler = new SendInvitationCommandHandler(_unitOfWork, _mapper, _clockMock.Object, _notificationServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateInvitationAndSendNotification_WhenInvitationIsSent()
        {
            // Arrange: Create two user profiles
            var sender = await AddAccountAsync("test1@email.com", "password1", "nick1");
            var receiver = await AddAccountAsync("test2@email.com", "password2", "nick2");
            var command = new SendInvitationCommand(sender.Profile.Id, receiver.Profile.Id);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert: Verify that the invitation was created
            Assert.NotNull(result);
            Assert.Equal(sender.Profile.Id, result.Sender.UserProfileId);
            Assert.Equal(receiver.Profile.Id, result.Receiver.UserProfileId);

            Assert.True(await _unitOfWork.FriendInvitations
                .IsFriendInvitationExistByProfileIdsAsync(sender.Profile.Id, receiver.Profile.Id, CancellationToken.None));

            // Assert: Verify that a notification was sent to the receiver
            _notificationServiceMock.Verify(ns => ns.CreateAndSendAsync(
                receiver.Profile.Id,
                NotificationTypeEnum.FriendRequestReceived,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldRenewInvitation_WhenExistingRejectedInvitationIsOlderThanSevenDays()
        {
            // Arrange
            var sender = await AddAccountAsync("test1@email.com", "password1", "nick1");
            var receiver = await AddAccountAsync("test2@email.com", "password2", "nick2");

            var invitation = await AddFriendInvitationAsync(sender.Profile.Id, receiver.Profile.Id);
            invitation.SetRejected(_fixedTestInstant.ToDateTimeUtc().AddDays(-8));

            _clockMock.Setup(clock => clock.GetCurrentInstant()).Returns(_fixedTestInstant);

            var command = new SendInvitationCommand(sender.Profile.Id, receiver.Profile.Id);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(sender.Profile.Id, result.Sender.UserProfileId);
            Assert.Equal(receiver.Profile.Id, result.Receiver.UserProfileId);
            Assert.Equal(FriendInvitationStatus.Pending.ToString(), result.Status);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenExistingRejectedInvitationIsWithinSevenDays()
        {
            // Arrange
            var sender = await AddAccountAsync("test1@email.com", "password1", "nick1");
            var receiver = await AddAccountAsync("test2@email.com", "password2", "nick2");

            var invitation = await AddFriendInvitationAsync(sender.Profile.Id, receiver.Profile.Id);
            invitation.SetRejected(_fixedTestInstant.ToDateTimeUtc().AddDays(-6));

            _clockMock.Setup(clock => clock.GetCurrentInstant()).Returns(_fixedTestInstant);

            var command = new SendInvitationCommand(sender.Profile.Id, receiver.Profile.Id);

            // Act & Assert
            await Assert.ThrowsAsync<FriendInvitationException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenSwappedUsersHavePendingInvitation()
        {
            // Arrange
            var sender = await AddAccountAsync("test1@email.com", "password1", "nick1");
            var receiver = await AddAccountAsync("test2@email.com", "password2", "nick2");

            var invitation = await AddFriendInvitationAsync(receiver.Profile.Id, sender.Profile.Id);

            _clockMock.Setup(clock => clock.GetCurrentInstant()).Returns(_fixedTestInstant);

            var command = new SendInvitationCommand(sender.Profile.Id, receiver.Profile.Id);

            // Act & Assert
            await Assert.ThrowsAsync<FriendInvitationException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenSwappedUsersHaveRejectedInvitationWithinSevenDays()
        {
            // Arrange
            var sender = await AddAccountAsync("test1@email.com", "password1", "nick1");
            var receiver = await AddAccountAsync("test2@email.com", "password2", "nick2");

            var invitation = await AddFriendInvitationAsync(receiver.Profile.Id, sender.Profile.Id);
            invitation.SetRejected(_fixedTestInstant.ToDateTimeUtc().AddDays(-6));

            _clockMock.Setup(clock => clock.GetCurrentInstant()).Returns(_fixedTestInstant);

            var command = new SendInvitationCommand(sender.Profile.Id, receiver.Profile.Id);

            // Act & Assert
            await Assert.ThrowsAsync<FriendInvitationException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldCreateNewInvitation_WhenSwappedUsersHaveRejectedInvitationOlderThanSevenDays()
        {
            // Arrange
            var sender = await AddAccountAsync("test1@email.com", "password1", "nick1");
            var receiver = await AddAccountAsync("test2@email.com", "password2", "nick2");

            var invitation = await AddFriendInvitationAsync(receiver.Profile.Id, sender.Profile.Id);
            invitation.SetRejected(_fixedTestInstant.ToDateTimeUtc().AddDays(-8));

            _clockMock.Setup(clock => clock.GetCurrentInstant()).Returns(_fixedTestInstant);

            var command = new SendInvitationCommand(sender.Profile.Id, receiver.Profile.Id);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(sender.Profile.Id, result.Sender.UserProfileId);
            Assert.Equal(receiver.Profile.Id, result.Receiver.UserProfileId);
            Assert.Equal(FriendInvitationStatus.Pending.ToString(), result.Status);
        }
    }
}
