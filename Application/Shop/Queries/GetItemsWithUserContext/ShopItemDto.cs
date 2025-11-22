namespace Application.Shop.Queries.GetItemsWithUserContext
{
    public record ShopItemDto(int Id, string Name, string Description, string ImageUrl, string ItemType, string Category, int Price, string CurrencyType, int LevelRequirement, UserContextDto UserContext);
}
