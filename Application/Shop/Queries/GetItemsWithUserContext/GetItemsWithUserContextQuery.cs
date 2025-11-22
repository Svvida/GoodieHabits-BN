using Application.Common.Interfaces;
using Application.Common.Sorting;
using Domain.Enums;

namespace Application.Shop.Queries.GetItemsWithUserContext
{
    public record GetItemsWithUserContextQuery(
        int UserProfileId,
        ShopItemsCategoryEnum? Category,
        ShopItemSortProperty SortBy = ShopItemSortProperty.Id,
        SortOrder SortOrder = SortOrder.Asc) : IQuery<List<ShopItemDto>>;
}
