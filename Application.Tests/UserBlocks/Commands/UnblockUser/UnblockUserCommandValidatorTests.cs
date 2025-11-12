using Application.UserBlocks.Commands.UnblockUser;
using FluentValidation.TestHelper;

namespace Application.Tests.UserBlocks.Commands.UnblockUser
{
    public class UnblockUserCommandValidatorTests : TestBase<UnblockUserCommandValidator>
    {
        private readonly UnblockUserCommandValidator _validator;

        public UnblockUserCommandValidatorTests()
        {
            _validator = new UnblockUserCommandValidator(_unitOfWork);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task ShouldHaveError_WhenBlockerUserProfileIdIsInvalid(int invalidId)
        {
            // Arrange
            var command = new UnblockUserCommand(invalidId, 2);
            // Act
            var result = await _validator.TestValidateAsync(command);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.BlockerUserProfileId)
                .WithErrorMessage("Blocker user profile ID must be greater than zero.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task ShouldHaveError_WhenBlockedUserProfileIdIsInvalid(int invalidId)
        {
            // Arrange
            var command = new UnblockUserCommand(1, invalidId);
            // Act
            var result = await _validator.TestValidateAsync(command);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.BlockedUserProfileId)
                .WithErrorMessage("Blocked user profile ID must be greater than zero.");
        }

        [Fact]
        public async Task ShouldHaveError_WhenUnblockingSelf()
        {
            // Arrange
            var command = new UnblockUserCommand(1, 1);
            // Act
            var result = await _validator.TestValidateAsync(command);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.BlockedUserProfileId)
                .WithErrorMessage("You cannot unblock yourself.");
        }

        [Fact]
        public async Task ShouldHaveError_WhenUserIsNotBlocked()
        {
            // Arrange: Create two user profiles but do not block one by the other
            var user1 = await AddAccountAsync("test1@email.com", "password1", "nickname1");
            var user2 = await AddAccountAsync("test2@email.com", "password2", "nickname2");
            var userBlock = await AddUserBlockAsync(user1.Profile.Id, user2.Profile.Id);

            var command = new UnblockUserCommand(user2.Profile.Id, user1.Profile.Id);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.BlockedUserProfileId)
                .WithErrorMessage("The user is not blocked.");
        }

        [Fact]
        public async Task ShouldNotHaveError_WhenUserIsBlocked()
        {
            // Arrange
            var user1 = await AddAccountAsync("test1@email.com", "password1", "nickname1");
            var user2 = await AddAccountAsync("test2@email.com", "password2", "nickname2");
            await AddUserBlockAsync(user1.Profile.Id, user2.Profile.Id);

            var command = new UnblockUserCommand(user1.Profile.Id, user2.Profile.Id);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
