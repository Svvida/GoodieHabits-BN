using Application.Common.Dtos;
using Application.Common.Interfaces.Notifications;
using Application.FriendInvitations.Commands.SendInvitation;
using Moq;

namespace Application.Tests.FriendInvitations.Commands
{
    public class SendInvitationCommandHandlerTests : TestBase<SendInvitationCommandHandler>
    {
        private readonly SendInvitationCommandHandler _handler;
        private readonly Mock<INotificationSender> _notificationSenderMock;

        public SendInvitationCommandHandlerTests()
        {
            _notificationSenderMock = new Mock<INotificationSender>();
            _handler = new SendInvitationCommandHandler(_unitOfWork, _mapper, _clockMock.Object, _notificationSenderMock.Object);
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
            _notificationSenderMock.Verify(ns => ns.SendNotificationAsync(
                receiver.Profile.Id,
                It.IsAny<NotificationDto>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
