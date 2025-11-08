using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IUserBlockRepository : IBaseRepository<UserBlock>
    {
        Task<bool> IsUserBlockExistsByProfileIdsAsync(int blockerUserProfileId, int blockedUserProfileId, CancellationToken cancellationToken = default);
        Task<UserBlock?> GetUserBlockByProfileIdsAsync(int blockerUserProfileId, int blockedUserProfileId, bool loadProfiles, CancellationToken cancellationToken = default);
    }
}
