using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IUserInventoryRepository : IBaseRepository<UserInventory>
    {
        Task<List<UserInventory>> GetUserInventoryItemsAsync(int userProfileId, CancellationToken cancellationToken = default);
    }
}
