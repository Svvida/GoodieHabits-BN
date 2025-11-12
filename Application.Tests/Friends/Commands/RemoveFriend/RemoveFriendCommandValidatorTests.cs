using Application.Friendships.Commands.RemoveFriend;
using FluentValidation.TestHelper;

namespace Application.Tests.Friends.Commands.RemoveFriend
{
    public class RemoveFriendCommandValidatorTests : TestBase<RemoveFriendCommandValidator>
    {
        private readonly RemoveFriendCommandValidator _validator;

        public RemoveFriendCommandValidatorTests()
        {
            _validator = new RemoveFriendCommandValidator(_unitOfWork);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task ShouldHaveError_WhenUserProfileIdIsInvalid(int invalidId)
        {
            // Arrange
            var command = new RemoveFriendCommand(1, invalidId);
            // Act
            var result = await _validator.TestValidateAsync(command);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UserProfileId)
                .WithErrorMessage("User Profile Id must be greater than 0.");
            result.ShouldHaveValidationErrorFor(x => x.FriendUserProfileId)
                .WithErrorMessage("Friendship does not exist.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task ShouldHaveError_WhenFriendUserProfileIdIsInvalid(int invalidId)
        {
            // Arrange
            var command = new RemoveFriendCommand(invalidId, 2);
            // Act
            var result = await _validator.TestValidateAsync(command);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.FriendUserProfileId)
                .WithErrorMessage("Friend User Profile Id must be greater than 0.");
            result.ShouldNotHaveValidationErrorFor(x => x.UserProfileId);
        }

        [Fact]
        public async Task ShouldNotHaveError_WhenFriendshipExists()
        {
            // Arrange: Create two user profiles and a friendship between them
            var user1 = await AddAccountAsync("email1@email.com", "hashed_password1", "nick1");
            var user2 = await AddAccountAsync("email2@email.com", "hashed_password2", "nick2");
            await AddFriendshipAsync(user1.Profile, user2.Profile);

            Assert.True(await _unitOfWork.Friends.IsFriendshipExistsByUserProfileIdsAsync(user1.Profile.Id, user2.Profile.Id, CancellationToken.None));

            var command = new RemoveFriendCommand(user1.Profile.Id, user2.Profile.Id);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.UserProfileId);
            result.ShouldNotHaveValidationErrorFor(x => x.FriendUserProfileId);
        }

        [Fact]
        public async Task ShouldHaveError_WhenFriendshipDoesntExists()
        {
            // Arrange: Create two user profiles without a friendship
            var user1 = await AddAccountAsync("email1@email.com", "hashed_password1", "nick1");
            var user2 = await AddAccountAsync("email2@email.com", "hashed_password2", "nick2");

            var command = new RemoveFriendCommand(user1.Profile.Id, user2.Profile.Id);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.FriendUserProfileId)
                  .WithErrorMessage("Friendship does not exist.");
        }
    }
}
