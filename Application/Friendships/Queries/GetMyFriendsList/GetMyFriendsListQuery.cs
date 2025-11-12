using Application.Common.Interfaces;

namespace Application.Friendships.Queries.GetMyFriendsList
{
    public record GetMyFriendsListQuery(int UserProfileId) : IQuery<List<FriendDto>>;
}
