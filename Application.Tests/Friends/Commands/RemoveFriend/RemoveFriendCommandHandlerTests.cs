using Application.Friendships.Commands.RemoveFriend;
using Domain.Models;
using NodaTime;

namespace Application.Tests.Friends.Commands.RemoveFriend
{
    public class RemoveFriendCommandHandlerTests : TestBase<RemoveFriendCommandHandler>
    {
        private readonly RemoveFriendCommandHandler _handler;
        private readonly Instant _fixedTestInstant;
        public RemoveFriendCommandHandlerTests()
        {
            _fixedTestInstant = Instant.FromUtc(2023, 10, 26, 10, 0, 0);
            _handler = new RemoveFriendCommandHandler(_unitOfWork);
        }
        [Fact]
        public async Task ShouldRemoveFriendship_WhenCommandIsValid()
        {
            // Arrange: Create two user profiles and a friendship between them
            var account1 = Account.Create("password1", "test1@email.com", "nickname1");
            var account2 = Account.Create("password2", "test2@email.com", "nickname2");
            _context.Accounts.AddRange(account1, account2);
            await _context.SaveChangesAsync();

            var friendship = Friendship.Create(account1.Profile.Id, account2.Profile.Id, _fixedTestInstant.ToDateTimeUtc());
            _context.Friendships.Add(friendship);
            await _context.SaveChangesAsync();

            var command = new RemoveFriendCommand(account1.Profile.Id, account2.Profile.Id);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert: Verify that the friendship has been removed
            var removedFriendship = await _unitOfWork.Friends.GetFriendshipByUserProfileIdsAsync(account1.Profile.Id, account2.Profile.Id, false, CancellationToken.None);
            Assert.Null(removedFriendship);
        }
    }
}
