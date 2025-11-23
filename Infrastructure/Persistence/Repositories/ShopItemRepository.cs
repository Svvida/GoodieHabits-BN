using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class ShopItemRepository(AppDbContext context) : BaseRepository<ShopItem>(context), IShopItemRepository
    {
        public IQueryable<ShopItem> GetAvailableItemsQuery(ShopItemsCategoryEnum? category)
        {
            var query = _context.ShopItems.AsQueryable();
            if (category.HasValue)
            {
                query = query.Where(item => item.Category == category.Value);
            }

            // Return only purchasable items
            query = query.Where(item => item.IsPurchasable);

            return query;
        }

        public async Task<IEnumerable<ShopItem>> GetFreeItemsUnlockableAtLevelAsync(int level, CancellationToken cancellationToken = default)
        {
            return await _context.ShopItems
                .Where(item => item.Price == 0 && item.LevelRequirement == level)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
