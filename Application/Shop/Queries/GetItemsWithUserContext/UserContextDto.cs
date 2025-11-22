namespace Application.Shop.Queries.GetItemsWithUserContext
{
    public record UserContextDto(
        bool IsOwned,
        int QuantityOwned,
        bool IsUnlocked,  // userLevel >= LevelRequirement
        bool CanAfford, // user.Coins >= Price
        bool CanPurchase, // final determination if user can purchase
        string? PurchaseLockReason);
}
