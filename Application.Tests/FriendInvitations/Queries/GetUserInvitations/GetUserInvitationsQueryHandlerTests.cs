using Application.FriendInvitations.Queries.GetUserInvitations;
using Domain.Enums;
using Moq;

namespace Application.Tests.FriendInvitations.Queries.GetUserInvitations
{
    public class GetUserInvitationsQueryHandlerTests : TestBase<GetUserInvitationsQueryHandler>
    {
        private readonly GetUserInvitationsQueryHandler _handler;

        public GetUserInvitationsQueryHandlerTests()
        {
            _handler = new GetUserInvitationsQueryHandler(_unitOfWork, _mapper);
        }

        [Fact]
        public async Task Handle_ShouldReturnSentInvitations()
        {
            // Arrange
            var sender = await AddAccountAsync("test1@email.com", "password1", "nickname1");

            var receiver = await AddAccountAsync("test2@email.com", "password2", "nickname2");
            receiver.Profile.UploadAvatar("receiver_avatar_id");
            await _context.SaveChangesAsync();

            var invitation = await AddFriendInvitationAsync(sender.Profile.Id, receiver.Profile.Id);

            Assert.True(await _unitOfWork.FriendInvitations.IsFriendInvitationExistByProfileIdsAsync(sender.Profile.Id, receiver.Profile.Id, CancellationToken.None));

            var query = new GetUserInvitationsQuery(sender.Profile.Id, InvitationDirection.Sent);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);

            var invitationDto = result.First();

            Assert.IsType<FriendInvitationDto>(invitationDto);

            Assert.Equal(invitation.Id, invitationDto.InvitationId);
            Assert.Equal(invitation.Status.ToString(), invitationDto.Status);
            Assert.Equal(_fixedTestInstant.ToDateTimeUtc(), invitation.CreatedAt);

            Assert.Equal(sender.Profile.Id, invitationDto.Sender.UserProfileId);
            Assert.Equal(sender.Profile.Nickname, invitationDto.Sender.Nickname);
            Assert.Empty(invitationDto.Sender.AvatarUrl);

            Assert.Equal(receiver.Profile.Id, invitationDto.Receiver.UserProfileId);
            Assert.Equal(receiver.Profile.Nickname, invitationDto.Receiver.Nickname);
            Assert.Equal("mock_url_for_receiver_avatar_id", invitationDto.Receiver.AvatarUrl);

            _urlBuilderMock.Verify(b => b.BuildThumbnailAvatarUrl(null), Times.Once);
            _urlBuilderMock.Verify(b => b.BuildThumbnailAvatarUrl("receiver_avatar_id"), Times.Once);
        }
    }
}
