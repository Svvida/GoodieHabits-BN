using Application.UserBlocks.Commands.BlockUser;

namespace Application.Tests.UserBlocks.Commands.BlockUser
{
    public class BlockUserCommandHandlerTests : TestBase<BlockUserCommandHandler>
    {
        private readonly BlockUserCommandHandler _handler;

        public BlockUserCommandHandlerTests()
        {
            _handler = new BlockUserCommandHandler(_unitOfWork, _clockMock.Object);
        }

        [Fact]
        public async Task ShouldBlockUserAndRemoveFriendshipIfExists_WhenCommandIsValid()
        {
            // Arrange: Create two user profiles
            var blocker = await AddAccountAsync("test1@email.com", "password1", "nick1");
            var blocked = await AddAccountAsync("test2@email.com", "password2", "nick2");

            await AddFriendshipAsync(blocker.Profile, blocked.Profile);

            Assert.Equal(1, blocked.Profile.FriendsCount);
            Assert.Equal(1, blocker.Profile.FriendsCount);
            Assert.True(await _unitOfWork.Friends.IsFriendshipExistsByUserProfileIdsAsync(blocker.Profile.Id, blocked.Profile.Id));

            var command = new BlockUserCommand(blocked.Profile.Id, blocker.Profile.Id);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert: Verify that the user block has been created
            Assert.True(await _unitOfWork.UserBlocks.IsUserBlockExistsByProfileIdsAsync(blocker.Profile.Id, blocked.Profile.Id));
            Assert.Equal(0, blocker.Profile.FriendsCount);
            Assert.Equal(0, blocked.Profile.FriendsCount);
            Assert.False(await _unitOfWork.Friends.IsFriendshipExistsByUserProfileIdsAsync(blocker.Profile.Id, blocked.Profile.Id));
        }

        [Fact]
        public async Task ShouldBlockUserAndRemoveFriendInvitationIfExists_WhenCommandIsValid()
        {
            // Arrange: Create two user profiles
            var blocker = await AddAccountAsync("test1@email.com", "password1", "nick1");
            var blocked = await AddAccountAsync("test2@email.com", "password2", "nick2");

            await AddFriendInvitationAsync(blocker.Profile.Id, blocked.Profile.Id);

            var command = new BlockUserCommand(blocked.Profile.Id, blocker.Profile.Id);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert: Verify that the user block has been created
            Assert.True(await _unitOfWork.UserBlocks.IsUserBlockExistsByProfileIdsAsync(blocker.Profile.Id, blocked.Profile.Id, CancellationToken.None));
            Assert.Null(await _unitOfWork.FriendInvitations.GetFriendInvitationByUserProfileIdsAsync(blocker.Profile.Id, blocked.Profile.Id, false, CancellationToken.None));
        }
    }
}
