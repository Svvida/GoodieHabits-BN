using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IUserInventoryRepository : IBaseRepository<UserInventory>
    {
        Task<List<UserInventory>> GetUserInventoryItemsAsync(int userProfileId, bool asNoTracking = true, bool includeUserProfile = false, CancellationToken cancellationToken = default);
        Task<UserInventory?> GetUserInventoryItemAsync(int userProfileId, int inventoryId, bool loadUserProfile, CancellationToken cancellationToken = default);
    }
}
