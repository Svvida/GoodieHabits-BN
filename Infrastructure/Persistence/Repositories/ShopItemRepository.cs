using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;

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
    }
}
