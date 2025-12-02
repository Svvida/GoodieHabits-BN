using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Inventories.Commands.EquipInventoryItem
{
    public class EquipInventoryItemCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<EquipInventoryItemCommand, Unit>
    {
        public async Task<Unit> Handle(EquipInventoryItemCommand request, CancellationToken cancellationToken)
        {
            var inventoryItems = await unitOfWork.UserInventories.GetUserInventoryItemsAsync(request.UserProfileId, false, true, cancellationToken).ConfigureAwait(false);

            var itemToEquip = inventoryItems.FirstOrDefault(ii => ii.Id == request.InventoryId)
                ?? throw new NotFoundException($"User Inventory item {request.InventoryId} not found.");

            if (itemToEquip.ShopItem.Category == ShopItemsCategoryEnum.Consumables)
            {
                throw new ConflictException("Consumable items cannot be equipped. Use the /use endpoint instead.");
            }

            if (itemToEquip.IsActive)
            {
                return Unit.Value;
            }

            var currentlyEquippedItem = inventoryItems
                .FirstOrDefault(ii =>
                    ii.ShopItem.Category == itemToEquip.ShopItem.Category &&
                    ii.IsActive);

            if (itemToEquip.ShopItem.Category == ShopItemsCategoryEnum.Avatars)
            {
                itemToEquip.UserProfile.EquipAvatar(itemToEquip);
            }
            else
            {
                currentlyEquippedItem?.Unequip();
                itemToEquip.Equip();
            }

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Unit.Value;
        }
    }
}
