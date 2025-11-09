using Application.FriendInvitations.Commands.UpdateInvitationStatus;
using Domain.Enums;
using FluentValidation.TestHelper;

namespace Application.Tests.FriendInvitations.Commands.UpdateInvitationStatus
{
    public class UpdateInvitationStatusCommandValidatorTests : TestBase<UpdateInvitationStatusCommandValidator>
    {
        private readonly UpdateInvitationStatusCommandValidator _validator;

        public UpdateInvitationStatusCommandValidatorTests()
        {
            _validator = new UpdateInvitationStatusCommandValidator(_unitOfWork);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task ShouldHaveError_WhenInvitationIdIsInvalid(int invalidInvitationId)
        {
            // Arrange
            var command = new UpdateInvitationStatusCommand(invalidInvitationId, 1, UpdateFriendInvitationStatusEnum.Accepted);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.InvitationId)
                .WithErrorMessage("InvitationId must be greater than 0.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task ShouldHaveError_WhenUserProfileIdIsInvalid(int invalidUserProfileId)
        {
            // Arrange
            var command = new UpdateInvitationStatusCommand(1, invalidUserProfileId, UpdateFriendInvitationStatusEnum.Accepted);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UserProfileId)
                .WithErrorMessage("UserProfileId must be greater than 0.");
        }

        [Fact]
        public async Task ShouldHaveError_WhenInvitationDoesNotExist()
        {
            // Arrange
            var command = new UpdateInvitationStatusCommand(999, 1, UpdateFriendInvitationStatusEnum.Accepted);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.InvitationId)
                .WithErrorMessage("Invitation doesn't exist or you don't have access to it.");
        }

        [Fact]
        public async Task ShouldHaveError_WhenStatusIsInvalid()
        {
            // Arrange
            var command = new UpdateInvitationStatusCommand(1, 1, (UpdateFriendInvitationStatusEnum)999);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Status)
                .WithErrorMessage("Status must be a valid enum value. 'Accepted', 'Rejected' or 'Cancelled'.");
        }

        [Fact]
        public async Task ShouldNotHaveAnyValidationErrors_WhenCommandIsValid()
        {
            // Arrange: Create a valid friend invitation
            var sender = await AddAccountAsync("sender@email.com", "password1", "senderNick");
            var receiver = await AddAccountAsync("receiver@email.com", "password2", "receiverNick");
            var invitation = await AddFriendInvitationAsync(sender.Profile.Id, receiver.Profile.Id);

            var command = new UpdateInvitationStatusCommand(invitation.Id, receiver.Profile.Id, UpdateFriendInvitationStatusEnum.Accepted);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
