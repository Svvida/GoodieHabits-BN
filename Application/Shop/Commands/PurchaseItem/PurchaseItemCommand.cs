using Application.Common.Interfaces;

namespace Application.Shop.Commands.PurchaseItem
{
    public record PurchaseItemCommand(int ShopItemId, int UserProfileId) : ICommand<PurchaseItemResponse>;
}
