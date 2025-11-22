using Domain.Enums;
using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IShopItemRepository : IBaseRepository<ShopItem>
    {
        IQueryable<ShopItem> GetAvailableItemsQuery(ShopItemsCategoryEnum? category);
    }
}
