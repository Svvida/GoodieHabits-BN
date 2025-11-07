using Application.Friendships.Commands.RemoveFriend;

namespace Application.Tests.Friends.Commands.RemoveFriend
{
    public class RemoveFriendCommandHandlerTests : TestBase<RemoveFriendCommandHandler>
    {
        private readonly RemoveFriendCommandHandler _handler;
        public RemoveFriendCommandHandlerTests()
        {
            _handler = new RemoveFriendCommandHandler(_unitOfWork);
        }
        [Fact]
        public async Task ShouldRemoveFriendship_WhenCommandIsValid()
        {
            // Arrange: Create two user profiles and a friendship between them
            var user1 = await AddAccountAsync("email1@email.com", "hashed_password1", "nick1");
            var user2 = await AddAccountAsync("email2@email.com", "hashed_password2", "nick2");
            await AddFriendshipAsync(user1.Profile.Id, user2.Profile.Id);

            var command = new RemoveFriendCommand(user1.Profile.Id, user2.Profile.Id);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert: Verify that the friendship has been removed
            var removedFriendship = await _unitOfWork.Friends.GetFriendshipByUserProfileIdsAsync(user1.Profile.Id, user2.Profile.Id, false, CancellationToken.None);
            Assert.Null(removedFriendship);
        }
    }
}
