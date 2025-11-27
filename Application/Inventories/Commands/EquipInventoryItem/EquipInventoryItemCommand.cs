using Application.Common.Interfaces;

namespace Application.Inventories.Commands.EquipInventoryItem
{
    public record EquipInventoryItemCommand(int UserProfileId, int InventoryId) : ICommand;
}
