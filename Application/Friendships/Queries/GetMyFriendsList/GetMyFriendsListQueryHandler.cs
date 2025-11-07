using Domain.Interfaces;
using MediatR;

namespace Application.Friendships.Queries.GetMyFriendsList
{
    public class GetMyFriendsListQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetMyFriendsListQuery, List<FriendDto>>
    {
        public async Task<List<FriendDto>> Handle(GetMyFriendsListQuery request, CancellationToken cancellationToken)
        {
            var friendships = await unitOfWork.Friends.GetUserFriendsAsync(request.UserProfileId, cancellationToken).ConfigureAwait(false);
            var friendsList = friendships.Select(f =>
            {
                var friendProfile = f.UserProfileId1 == request.UserProfileId ? f.UserProfile2 : f.UserProfile1;
                return new FriendDto(friendProfile.Id, friendProfile.Nickname, friendProfile.Avatar);
            }).ToList();
            return friendsList;
        }
    }
}
