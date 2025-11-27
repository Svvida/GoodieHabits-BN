using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using NodaTime;

namespace Application.Inventories.Commands.UseInventoryItem
{
    public class UseInventoryItemCommandHandler(IUnitOfWork unitOfWork, IClock clock) : IRequestHandler<UseInventoryItemCommand, Unit>
    {
        public async Task<Unit> Handle(UseInventoryItemCommand request, CancellationToken cancellationToken)
        {
            var itemToUse = await unitOfWork.UserInventories.GetUserInventoryItemAsync(request.UserProfileId, request.InventoryId, true, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"User Inventory item {request.InventoryId} not found.");

            if (itemToUse.ShopItem.Category != ShopItemsCategoryEnum.Consumables)
                throw new ConflictException("Only consumable items can be used.");

            itemToUse.UserProfile.UseConsumableItem(itemToUse, clock.GetCurrentInstant().ToDateTimeUtc());

            if (itemToUse.Quantity <= 0)
            {
                unitOfWork.UserInventories.Remove(itemToUse);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Unit.Value;
        }
    }
}
