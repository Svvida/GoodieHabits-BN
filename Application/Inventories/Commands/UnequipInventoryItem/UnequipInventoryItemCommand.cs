using Application.Common.Interfaces;

namespace Application.Inventories.Commands.UnequipInventoryItem
{
    public record UnequipInventoryItemCommand(int UserProfileId, int InventoryId) : ICommand;
}
