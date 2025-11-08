using Application.FriendInvitations.Commands.SendInvitation;
using FluentValidation.TestHelper;

namespace Application.Tests.FriendInvitations.Commands
{
    public class SendInvitationCommandValidatorTests : TestBase<SendInvitationCommandValidator>
    {
        private readonly SendInvitationCommandValidator _validator;

        public SendInvitationCommandValidatorTests()
        {
            _validator = new SendInvitationCommandValidator(_unitOfWork);
        }

        [Fact]
        public async Task Validate_ShouldHaveError_WhenReceiverDoesNotExist()
        {
            // Arrange
            var command = new SendInvitationCommand(1, 2);
            // Act
            var result = await _validator.TestValidateAsync(command);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ReceiverUserProfileId)
                .WithErrorMessage("Receiver user profile not found.");
        }

        [Fact]
        public async Task Validate_ShouldNotHaveError_WhenReceiverExistsAndEligible()
        {
            // Arrange: Create two user profiles
            var sender = await AddAccountAsync("test1@email.com", "password1", "nickname1");
            var receiver = await AddAccountAsync("test2@email.com", "password2", "nickname2");

            var command = new SendInvitationCommand(sender.Profile.Id, receiver.Profile.Id);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ShouldHaveError_WhenUsersAreFriends()
        {
            // Arrange
            var sender = await AddAccountAsync("test1@email.com", "password1", "nickname1");
            var receiver = await AddAccountAsync("test2@email.com", "password2", "nickname2");

            await AddFriendshipAsync(sender.Profile, receiver.Profile);

            var command = new SendInvitationCommand(sender.Profile.Id, receiver.Profile.Id);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ReceiverUserProfileId)
                .WithErrorMessage("Unable to send invitation. Reason: AlreadyFriends.");
        }

        [Fact]
        public async Task Validate_ShouldHaveError_WhenPendingInvitationExists()
        {
            // Arrange
            var sender = await AddAccountAsync("test1@email.com", "password1", "nickname1");
            var receiver = await AddAccountAsync("test2@email.com", "password2", "nickname2");

            await AddFriendInvitationAsync(sender.Profile.Id, receiver.Profile.Id);

            var command = new SendInvitationCommand(sender.Profile.Id, receiver.Profile.Id);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ReceiverUserProfileId)
                .WithErrorMessage("Unable to send invitation. Reason: InvitationExists.");
        }

        [Fact]
        public async Task Validate_ShouldHaveError_WhenSenderBlockedReceiver()
        {
            // Arrange
            var sender = await AddAccountAsync("test1@email.com", "password1", "nickname1");
            var receiver = await AddAccountAsync("test2@email.com", "password2", "nickname2");

            await AddUserBlockAsync(sender.Profile.Id, receiver.Profile.Id);

            var command = new SendInvitationCommand(sender.Profile.Id, receiver.Profile.Id);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ReceiverUserProfileId)
                .WithErrorMessage("Unable to send invitation. Reason: SenderIsBlocking.");
        }

        [Fact]
        public async Task Validate_ShouldHaveError_WhenReceiverBlockedSender()
        {
            // Arrange
            var sender = await AddAccountAsync("test1@email.com", "password1", "nickname1");
            var receiver = await AddAccountAsync("test2@email.com", "password2", "nickname2");

            await AddUserBlockAsync(receiver.Profile.Id, sender.Profile.Id);

            var command = new SendInvitationCommand(sender.Profile.Id, receiver.Profile.Id);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ReceiverUserProfileId)
                .WithErrorMessage("Unable to send invitation. Reason: BlockedByRecipient.");
        }
    }
}