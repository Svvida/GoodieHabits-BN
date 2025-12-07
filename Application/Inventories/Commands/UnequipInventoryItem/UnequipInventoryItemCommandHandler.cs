using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Inventories.Commands.UnequipInventoryItem
{
    public class UnequipInventoryItemCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UnequipInventoryItemCommand, Unit>
    {
        public async Task<Unit> Handle(UnequipInventoryItemCommand request, CancellationToken cancellationToken)
        {
            var itemToUnequip = await unitOfWork.UserInventories.GetUserInventoryItemAsync(request.UserProfileId, request.InventoryId, true, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"User Inventory item {request.InventoryId} not found.");

            if (itemToUnequip.ShopItem.Category == ShopItemsCategoryEnum.Consumables)
            {
                throw new ConflictException("Consumable items cannot be equipped. Use the /use endpoint instead.");
            }

            if (!itemToUnequip.IsActive)
            {
                return Unit.Value;
            }

            if (itemToUnequip.ShopItem.Category == ShopItemsCategoryEnum.Avatars)
                itemToUnequip.UserProfile.UnequipAvatar(itemToUnequip);
            else
                itemToUnequip.Unequip();
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Unit.Value;
        }
    }
}
