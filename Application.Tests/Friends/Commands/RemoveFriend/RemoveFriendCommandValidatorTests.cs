using Application.Friendships.Commands.RemoveFriend;
using Domain.Models;
using FluentValidation.TestHelper;
using NodaTime;

namespace Application.Tests.Friends.Commands.RemoveFriend
{
    public class RemoveFriendCommandValidatorTests : TestBase<RemoveFriendCommandValidator>
    {
        private readonly RemoveFriendCommandValidator _validator;
        private readonly Instant _fixedTestInstant;

        public RemoveFriendCommandValidatorTests()
        {
            _fixedTestInstant = Instant.FromUtc(2023, 10, 26, 10, 0, 0);
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
            var user1 = Account.Create("hashed_password1", "email1@email.com", "nick1");
            var user2 = Account.Create("hashed_password2", "email2@email.com", "nick2");
            _context.Accounts.AddRange(user1, user2);
            await _context.SaveChangesAsync();
            var friendship = Friendship.Create(user1.Profile.Id, user2.Profile.Id, _fixedTestInstant.ToDateTimeUtc());
            _context.Friendships.Add(friendship);
            await _context.SaveChangesAsync();

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
            var user1 = Account.Create("hashed_password1", "email1@email.com", "nick1");
            var user2 = Account.Create("hashed_password2", "email2@email.com", "nick2");
            _context.Accounts.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var command = new RemoveFriendCommand(user1.Profile.Id, user2.Profile.Id);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.FriendUserProfileId)
                  .WithErrorMessage("Friendship does not exist.");
        }
    }
}
