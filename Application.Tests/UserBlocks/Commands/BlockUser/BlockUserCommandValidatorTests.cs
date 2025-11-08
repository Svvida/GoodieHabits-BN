using Application.UserBlocks.Commands.BlockUser;
using FluentValidation.TestHelper;

namespace Application.Tests.UserBlocks.Commands.BlockUser
{
    public class BlockUserCommandValidatorTests : TestBase<BlockUserCommandValidator>
    {
        private readonly BlockUserCommandValidator _validator;
        public BlockUserCommandValidatorTests()
        {
            _validator = new BlockUserCommandValidator(_unitOfWork);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task ShouldHaveError_WhenBlockerUserProfileIdIsInvalid(int invalidId)
        {
            // Arrange
            var command = new BlockUserCommand(2, invalidId);
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
            var command = new BlockUserCommand(invalidId, 1);
            // Act
            var result = await _validator.TestValidateAsync(command);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.BlockedUserProfileId)
                .WithErrorMessage("Blocked user profile ID must be greater than zero.");
        }

        [Fact]
        public async Task ShouldHaveError_WhenBlockingSelf()
        {
            // Arrange
            var command = new BlockUserCommand(1, 1);
            // Act
            var result = await _validator.TestValidateAsync(command);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.BlockedUserProfileId)
                .WithErrorMessage("You cannot block yourself.");
        }

        [Fact]
        public async Task ShouldHaveError_WhenUserIsAlreadyBlocked()
        {
            // Arrange: Create two user profiles and block one by the other
            var user1 = await AddAccountAsync("test1@email.com", "password1", "nickname1");
            var user2 = await AddAccountAsync("test2@email.com", "password2", "nickname2");
            var userBlock = await AddUserBlockAsync(user1.Profile.Id, user2.Profile.Id);

            var command = new BlockUserCommand(user2.Profile.Id, user1.Profile.Id);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.BlockedUserProfileId)
                .WithErrorMessage("The user is already blocked.");
        }
    }
}