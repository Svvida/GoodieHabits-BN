using Application.Inventories.Commands.EquipInventoryItem;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests.Inventories.Commands.EquipInventoryItem
{
    public class EquipInventoryItemCommandHandlerTests : TestBase<EquipInventoryItemCommandHandler>
    {
        private readonly EquipInventoryItemCommandHandler _handler;

        public EquipInventoryItemCommandHandlerTests()
        {
            _handler = new EquipInventoryItemCommandHandler(_unitOfWork);
        }

        [Fact]
        public async Task Handle_ShouldEquipItem_WhenNoItemEquippedInSlot()
        {
            // Arrange
            var user = await CreateUserWithLevelAndCoins(1, 0);

            // Create a Hat
            var hatItem = await GetOrCreateShopItemAsync(9001, "Cool Hat", ShopItemsCategoryEnum.Avatars, ShopItemTypeEnum.Cosmetic, payload: new AvatarPayload { AvatarId = "hat" });
            var inventoryItem = await AddUserInventoryItemAsync(user.Profile.Id, hatItem.Id, 1);

            var command = new EquipInventoryItemCommand(user.Profile.Id, inventoryItem.Id);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            var dbItem = await _unitOfWork.UserInventories.GetUserInventoryItemAsync(user.Id, inventoryItem.Id, false, CancellationToken.None);
            dbItem!.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ShouldSwapItems_WhenItemAlreadyEquippedInSameCategory()
        {
            // Arrange
            var user = await CreateUserWithLevelAndCoins(1, 0);

            // Create Two Frames (Same Category)
            var frameA = await GetOrCreateShopItemAsync(9010, "Frame A", ShopItemsCategoryEnum.AvatarFrames, ShopItemTypeEnum.Cosmetic, payload: new AvatarFramePayload { FrameId = "a" });
            var frameB = await GetOrCreateShopItemAsync(9011, "Frame B", ShopItemsCategoryEnum.AvatarFrames, ShopItemTypeEnum.Cosmetic, payload: new AvatarFramePayload { FrameId = "b" });

            // User owns both
            var invItemA = await AddUserInventoryItemAsync(user.Profile.Id, frameA.Id, 1);
            var invItemB = await AddUserInventoryItemAsync(user.Profile.Id, frameB.Id, 1);

            // Setup: Item A is ALREADY equipped
            invItemA.Equip();
            await _unitOfWork.SaveChangesAsync();

            var command = new EquipInventoryItemCommand(user.Profile.Id, invItemB.Id);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            var dbItemA = await _context.UserInventories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == invItemA.Id);
            var dbItemB = await _context.UserInventories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == invItemB.Id);

            dbItemA!.IsActive.Should().BeFalse("Item A should be unequipped");
            dbItemB!.IsActive.Should().BeTrue("Item B should be equipped");
        }

        [Fact]
        public async Task Handle_ShouldThrowConflict_WhenEquippingConsumable()
        {
            // Arrange
            var user = await CreateUserWithLevelAndCoins(1, 0);
            var potion = await GetOrCreateShopItemAsync(9020, "Potion", ShopItemsCategoryEnum.Consumables, ShopItemTypeEnum.Consumable, payload: new ConsumablePayload());
            var invItem = await AddUserInventoryItemAsync(user.Profile.Id, potion.Id, 1);

            var command = new EquipInventoryItemCommand(user.Profile.Id, invItem.Id);

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFound_WhenUserDoesNotOwnItem()
        {
            // Arrange
            var user = await CreateUserWithLevelAndCoins(1, 0);
            var otherUser = await AddAccountAsync("other@test.com", "pw", "Other");

            var item = await GetOrCreateShopItemAsync(9030, "Sword", ShopItemsCategoryEnum.Avatars, ShopItemTypeEnum.Cosmetic);

            // Give item to Other User
            var otherInvItem = await AddUserInventoryItemAsync(otherUser.Profile.Id, item.Id, 1);

            var command = new EquipInventoryItemCommand(user.Profile.Id, otherInvItem.Id); // Wrong User ID

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }
    }
}