using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class UserInventoryRepository(AppDbContext context) : BaseRepository<UserInventory>(context), IUserInventoryRepository
    {
        public async Task<List<UserInventory>> GetUserInventoryItemsAsync(int userProfileId, CancellationToken cancellationToken = default)
        {
            return await _context.UserInventories
                .Where(ui => ui.UserProfileId == userProfileId)
                .Include(ui => ui.ShopItem)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<UserInventory?> GetUserInventoryItemAsync(int userProfileId, int inventoryId, bool loadUserProfile = false, CancellationToken cancellationToken = default)
        {
            var query = _context.UserInventories
                .Include(ui => ui.ShopItem)
                .Where(ui => ui.UserProfileId == userProfileId && ui.Id == inventoryId);

            if (loadUserProfile)
            {
                query = query.Include(ui => ui.UserProfile);
            }

            return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
