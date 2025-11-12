using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IFriendsRepository : IBaseRepository<Friendship>
    {
        Task<List<Friendship>> GetUserFriendsAsync(int userProfileId, CancellationToken cancellationToken = default);
        Task<Friendship?> GetFriendshipByUserProfileIdsAsync(int userProfileId1, int userProfileId2, bool loadProfiles, CancellationToken cancellationToken = default);
        Task<bool> IsFriendshipExistsByUserProfileIdsAsync(int userProfileId1, int userProfileId2, CancellationToken cancellationToken = default);
    }
}
