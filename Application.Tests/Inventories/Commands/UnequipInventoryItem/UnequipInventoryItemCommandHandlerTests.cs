using Application.Inventories.Commands.UnequipInventoryItem;
using FluentAssertions;

namespace Application.Tests.Inventories.Commands.UnequipInventoryItem
{
    public class UnequipInventoryItemCommandHandlerTests : TestBase<UnequipInventoryItemCommandHandler>
    {
        private readonly UnequipInventoryItemCommandHandler _handler;
        public UnequipInventoryItemCommandHandlerTests()
        {
            _handler = new UnequipInventoryItemCommandHandler(_unitOfWork);
        }

        [Fact]
        public async Task Handle_ShouldUnequipItem_WhenItemIsEquipped()
        {
            // Arrange
            var user = await CreateUserWithLevelAndCoins(1, 0);
            // Create a Hat
            var hatItem = await GetOrCreateShopItemAsync(9001, "Cool Hat", Domain.Enums.ShopItemsCategoryEnum.Avatars, Domain.Enums.ShopItemTypeEnum.Cosmetic, payload: new Domain.ValueObjects.AvatarPayload { AvatarId = "hat" });
            var inventoryItem = await AddUserInventoryItemAsync(user.Profile.Id, hatItem.Id, 1);
            // Equip the item first
            inventoryItem.Equip();
            await _unitOfWork.SaveChangesAsync();
            var command = new UnequipInventoryItemCommand(user.Profile.Id, inventoryItem.Id);
            // Act
            await _handler.Handle(command, CancellationToken.None);
            // Assert
            var dbItem = await _unitOfWork.UserInventories.GetUserInventoryItemAsync(user.Id, inventoryItem.Id, false, CancellationToken.None);
            dbItem!.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ShouldDoNothing_WhenItemIsAlreadyUnequipped()
        {
            // Arrange
            var user = await CreateUserWithLevelAndCoins(1, 0);
            // Create a Hat
            var hatItem = await GetOrCreateShopItemAsync(9001, "Cool Hat", Domain.Enums.ShopItemsCategoryEnum.Avatars, Domain.Enums.ShopItemTypeEnum.Cosmetic, payload: new Domain.ValueObjects.AvatarPayload { AvatarId = "hat" });
            var inventoryItem = await AddUserInventoryItemAsync(user.Profile.Id, hatItem.Id, 1);
            // Ensure the item is UNEQUIPPED
            inventoryItem.Unequip();
            await _unitOfWork.SaveChangesAsync();
            var command = new UnequipInventoryItemCommand(user.Profile.Id, inventoryItem.Id);
            // Act
            await _handler.Handle(command, CancellationToken.None);
            // Assert
            var dbItem = await _unitOfWork.UserInventories.GetUserInventoryItemAsync(user.Id, inventoryItem.Id, false, CancellationToken.None);
            dbItem!.IsActive.Should().BeFalse(); // Still unequipped
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenItemDoesNotExist()
        {
            // Arrange
            var user = await CreateUserWithLevelAndCoins(1, 0);
            var command = new UnequipInventoryItemCommand(user.Profile.Id, 999);
            // Act & Assert
            await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
                .Should().ThrowAsync<Domain.Exceptions.NotFoundException>()
                .WithMessage($"User Inventory item {command.InventoryId} not found.");
        }
    }
}
