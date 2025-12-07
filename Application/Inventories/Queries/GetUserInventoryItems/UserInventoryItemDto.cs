namespace Application.Inventories.Queries.GetUserInventoryItems
{
    public record UserInventoryItemDto(
        int UserInventoryId,
        int ShopItemId,
        string ItemType,
        string Category,
        int Quantity,
        DateTime AcquiredAt,
        bool IsActive,
        string ItemUrl,
        string ItemName,
        string ItemDescription);
}
