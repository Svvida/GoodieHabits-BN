using Application.UserBlocks.Commands.UnblockUser;

namespace Application.Tests.UserBlocks.Commands.UnblockUser
{
    public class UnblockUserCommandHandlerTests : TestBase<UnblockUserCommandHandler>
    {
        private readonly UnblockUserCommandHandler _handler;

        public UnblockUserCommandHandlerTests()
        {
            _handler = new UnblockUserCommandHandler(_unitOfWork);
        }

        [Fact]
        public async Task ShouldUnblockUser_WhenCommandIsValid()
        {
            // Arrange: Create two user profiles and block one by the other
            var blocker = await AddAccountAsync("test1@email.com", "password1", "nickname1");
            var blocked = await AddAccountAsync("test2@email.com", "password2", "nickname2");
            await AddUserBlockAsync(blocker.Profile.Id, blocked.Profile.Id);

            Assert.True(await _unitOfWork.UserBlocks.IsUserBlockExistsByProfileIdsAsync(blocker.Profile.Id, blocked.Profile.Id, CancellationToken.None));

            var command = new UnblockUserCommand(blocker.Profile.Id, blocked.Profile.Id);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(await _unitOfWork.UserBlocks.IsUserBlockExistsByProfileIdsAsync(blocker.Profile.Id, blocked.Profile.Id, CancellationToken.None));
        }
    }
}
