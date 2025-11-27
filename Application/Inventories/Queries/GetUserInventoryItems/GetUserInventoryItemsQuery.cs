using Application.Common.Interfaces;

namespace Application.Inventories.Queries.GetUserInventoryItems
{
    public record GetUserInventoryItemsQuery(int UserProfileId) : IQuery<List<UserInventoryItemDto>>;
}
