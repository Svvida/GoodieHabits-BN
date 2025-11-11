using Application.UserProfiles.Queries.GetUserProfileForPublicDisplay;
using Domain.Enums;

namespace Application.Tests.UserProfiles.Queries.GetUserProfileForPublicDisplay
{
    public class GetUserProfileForPublicDisplayQueryHandlerTests : TestBase<GetUserProfileForPublicDisplayQueryHandler>
    {
        private readonly GetUserProfileForPublicDisplayQueryHandler _handler;

        public GetUserProfileForPublicDisplayQueryHandlerTests()
        {
            _handler = new GetUserProfileForPublicDisplayQueryHandler(_unitOfWork, _mapper);
        }

        [Fact]
        public async Task Handle_ShouldReturnPublicUserProfileDto_WhenUsersAreNotFriendsAndNoInvitationExists()
        {
            // Arrange
            var currentUser = await AddAccountAsync("current@example.com", "password", "CurrentUser");
            var viewedUser = await AddAccountAsync("viewed@example.com", "password", "ViewedUser");

            var query = new GetUserProfileForPublicDisplayQuery(currentUser.Profile.Id, viewedUser.Profile.Id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsType<PublicUserProfileDto>(result);
            var dto = (PublicUserProfileDto)result;
            Assert.Equal(viewedUser.Profile.Id, dto.UserProfileId);
            Assert.Equal(viewedUser.Profile.Nickname, dto.Nickname);
            Assert.Equal(FriendshipStatus.NotFriends, dto.FriendshipStatus);
            Assert.True(dto.CanSendInvitation);
        }

        [Fact]
        public async Task Handle_ShouldReturnFriendUserProfileDto_WhenUsersAreFriends()
        {
            // Arrange
            var currentUser = await AddAccountAsync("current@example.com", "password", "CurrentUser");
            var viewedUser = await AddAccountAsync("viewed@example.com", "password", "ViewedUser");

            await AddFriendshipAsync(currentUser.Profile, viewedUser.Profile);

            var query = new GetUserProfileForPublicDisplayQuery(currentUser.Profile.Id, viewedUser.Profile.Id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsType<FriendUserProfileDto>(result);
            var dto = (FriendUserProfileDto)result;
            Assert.Equal(viewedUser.Profile.Id, dto.UserProfileId);
            Assert.Equal(viewedUser.Profile.Nickname, dto.Nickname);
            Assert.Equal(FriendshipStatus.Friends, dto.FriendshipStatus);
        }

        [Fact]
        public async Task Handle_ShouldReturnBlockingUserProfileDto_WhenCurrentUserIsBlockingViewedUser()
        {
            // Arrange
            var currentUser = await AddAccountAsync("current@example.com", "password", "CurrentUser");
            var viewedUser = await AddAccountAsync("viewed@example.com", "password", "ViewedUser");

            await AddUserBlockAsync(currentUser.Profile.Id, viewedUser.Profile.Id);

            var query = new GetUserProfileForPublicDisplayQuery(currentUser.Profile.Id, viewedUser.Profile.Id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsType<BlockingUserProfileDto>(result);
            var dto = (BlockingUserProfileDto)result;
            Assert.Equal(viewedUser.Profile.Id, dto.UserProfileId);
            Assert.Equal(viewedUser.Profile.Nickname, dto.Nickname);
            Assert.Equal(FriendshipStatus.Blocking, dto.FriendshipStatus);
        }

        [Fact]
        public async Task Handle_ShouldReturnBlockedByUserProfileDto_WhenCurrentUserIsBlockedByViewedUser()
        {
            // Arrange
            var currentUser = await AddAccountAsync("current@example.com", "password", "CurrentUser");
            var viewedUser = await AddAccountAsync("viewed@example.com", "password", "ViewedUser");

            await AddUserBlockAsync(viewedUser.Profile.Id, currentUser.Profile.Id);

            var query = new GetUserProfileForPublicDisplayQuery(currentUser.Profile.Id, viewedUser.Profile.Id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsType<BlockedByUserProfileDto>(result);
            var dto = (BlockedByUserProfileDto)result;
            Assert.Equal(viewedUser.Profile.Id, dto.UserProfileId);
            Assert.Equal(viewedUser.Profile.Nickname, dto.Nickname);
            Assert.Equal(FriendshipStatus.BlockedBy, dto.FriendshipStatus);
        }
    }
}
