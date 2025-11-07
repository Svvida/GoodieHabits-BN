using Application.Friendships.Queries.GetMyFriendsList;

namespace Application.Tests.Friends.Queries.GetMyFriendsList
{
    public class GetMyFriendsListQueryHandlerTests : TestBase<GetMyFriendsListQueryHandler>
    {
        private readonly GetMyFriendsListQueryHandler _handler;

        public GetMyFriendsListQueryHandlerTests()
        {
            _handler = new GetMyFriendsListQueryHandler(_unitOfWork);
        }

        [Fact]
        public async Task Handle_ShouldReturnFriendsList()
        {
            // Arrange
            var account1 = await AddAccountAsync("test1@email.com", "password1", "nickname1");
            var account2 = await AddAccountAsync("test2@email.com", "password2", "nickname2");

            var friendship = await AddFriendshipAsync(account1.Profile.Id, account2.Profile.Id);

            var query = new GetMyFriendsListQuery(account1.Profile.Id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);

            Assert.Single(result);

            var friendDto = result.First();

            Assert.IsType<FriendDto>(friendDto);

            Assert.Equal(account2.Profile.Id, friendDto.UserProfileId);
            Assert.Equal(account2.Profile.Nickname, friendDto.Nickname);
            Assert.Null(friendDto.AvatarUrl);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoFriendsExist()
        {
            // Arrange
            var account1 = await AddAccountAsync("lonely@email.com", "pw", "lonely_user");
            // No friendship added
            var query = new GetMyFriendsListQuery(account1.Profile.Id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
