using Application.UserProfiles.Queries.GetUserProfiles;
using FluentAssertions;

namespace Application.Tests.UserProfiles.Queries.GetUserProfiles
{
    public class GetUserProfilesQueryHandlerTests : TestBase<GetUserProfilesQueryHandler>
    {
        private readonly GetUserProfilesQueryHandler _handler;

        public GetUserProfilesQueryHandlerTests()
        {
            _handler = new GetUserProfilesQueryHandler(_unitOfWork, _urlBuilderMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnMappedUserProfiles_WhenProfilesExist()
        {
            // Arrange
            var account1 = await AddAccountAsync("user1@example.com", "hashed_password1", "UserOne");
            var account2 = await AddAccountAsync("user2@example.com", "hashed_password2", "SecondUser");
            var account3 = await AddAccountAsync("user3@example.com", "hashed_password3", "Other");

            account1.Profile.Avatar = "avatar1";
            account2.Profile.Avatar = "avatar2";
            account3.Profile.Avatar = "avatar3";

            await _context.SaveChangesAsync();

            var query = new GetUserProfilesQuery("User", 10);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2); // Only "UserOne" and "SecondUser" match the nickname filter

            result.Should().ContainSingle(p => p.Nickname == "UserOne" && p.AvatarUrl == "mock_url_for_avatar1");
            result.Should().ContainSingle(p => p.Nickname == "SecondUser" && p.AvatarUrl == "mock_url_for_avatar2");
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoProfilesMatch()
        {
            // Arrange
            await AddAccountAsync("user1@example.com", "hashed_password1", "UserOne");
            await AddAccountAsync("user2@example.com", "hashed_password2", "UserTwo");

            await _context.SaveChangesAsync();

            var query = new GetUserProfilesQuery("NonExistentNickname", 10);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ShouldRespectLimit_WhenProfilesExceedLimit()
        {
            // Arrange
            await AddAccountAsync("user1@example.com", "hashed_password1", "UserOne");
            await AddAccountAsync("user2@example.com", "hashed_password2", "UserTwo");
            await AddAccountAsync("user3@example.com", "hashed_password3", "UserThree");

            await _context.SaveChangesAsync();

            var query = new GetUserProfilesQuery(null, 2); // Limit to 2 results

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2); // Only 2 profiles should be returned
        }

        [Fact]
        public async Task Handle_ShouldReturnAllProfiles_WhenNoNicknameFilter()
        {
            // Arrange
            await AddAccountAsync("user1@example.com", "hashed_password1", "UserOne");
            await AddAccountAsync("user2@example.com", "hashed_password2", "UserTwo");

            await _context.SaveChangesAsync();

            var query = new GetUserProfilesQuery(null, 10); // No nickname filter

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2); // All profiles should be returned
        }
    }
}
