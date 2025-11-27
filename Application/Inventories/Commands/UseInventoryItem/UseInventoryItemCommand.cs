using Application.Common.Interfaces;

namespace Application.Inventories.Commands.UseInventoryItem
{
    public record UseInventoryItemCommand(int UserProfileId, int InventoryId) : ICommand;
}
