using System.Reflection;
using Application.Shop.Commands.PurchaseItem;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;

namespace Application.Tests.Shop.Commands.PurchaseItem
{
    public class PurchaseItemCommandHandlerTests : TestBase<PurchaseItemCommandHandler>
    {
        private readonly PurchaseItemCommandHandler _handler;
        public PurchaseItemCommandHandlerTests()
        {
            _handler = new PurchaseItemCommandHandler(_unitOfWork, _clockMock.Object, _levelCalculatorMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldPurchaseUniqueItem_WhenConditionsMet()
        {
            // Arrange
            // User Level 10, 1000 Coins
            var userAccount = await CreateUserWithLevelAndCoins(10, 1000);
            // Item Cost 500, Req Level 5, Unique
            var item = await AddShopItemAsync("Unique Hat", 500, levelReq: 5, isUnique: true);

            var command = new PurchaseItemCommand(item.Id, userAccount.Profile.Id);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(500, result.Coins); // 1000 - 500

            // Verify DB state
            var userFromDb = await _unitOfWork.UserProfiles.GetUserProfileWithInventoryItemsForShopContextAsync(userAccount.Profile.Id, true, CancellationToken.None);
            Assert.NotNull(userFromDb);
            Assert.Equal(500, userFromDb.Coins);
            Assert.Single(userFromDb.InventoryItems);
            Assert.Equal(item.Id, userFromDb.InventoryItems.First().ShopItemId);
            Assert.Equal(1, userFromDb.InventoryItems.First().Quantity);
        }

        [Fact]
        public async Task Handle_ShouldPurchaseConsumableItem_AndCreateNewStack_WhenNotOwned()
        {
            // Arrange
            var userAccount = await CreateUserWithLevelAndCoins(10, 200);
            // Item Cost 50, Not Unique
            var item = await AddShopItemAsync("Potion", 50, levelReq: 1, isUnique: false);

            var command = new PurchaseItemCommand(item.Id, userAccount.Profile.Id);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(150, result.Coins);

            var userFromDb = await _unitOfWork.UserProfiles.GetUserProfileWithInventoryItemsForShopContextAsync(userAccount.Profile.Id, true, CancellationToken.None);
            Assert.NotNull(userFromDb);
            Assert.Single(userFromDb.InventoryItems);
            Assert.Equal(1, userFromDb.InventoryItems.First().Quantity);
        }

        [Fact]
        public async Task Handle_ShouldPurchaseConsumableItem_AndIncrementStack_WhenAlreadyOwned()
        {
            // Arrange
            var userAccount = await CreateUserWithLevelAndCoins(10, 200);
            var item = await AddShopItemAsync("Potion", 50, levelReq: 1, isUnique: false);

            // Pre-seed inventory with 1 potion
            await AddUserInventoryItemAsync(userAccount.Profile.Id, item.Id, 1);

            var command = new PurchaseItemCommand(item.Id, userAccount.Profile.Id);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            var userFromDb = await _unitOfWork.UserProfiles.GetUserProfileWithInventoryItemsForShopContextAsync(userAccount.Profile.Id, true, CancellationToken.None);
            Assert.NotNull(userFromDb);
            Assert.Equal(150, userFromDb.Coins);
            Assert.Single(userFromDb.InventoryItems); // Still only 1 row
            Assert.Equal(2, userFromDb.InventoryItems.First().Quantity); // Quantity increased to 2
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenInsufficientFunds()
        {
            // Arrange
            var userAccount = await CreateUserWithLevelAndCoins(10, 50); // Only 50 coins
            var item = await AddShopItemAsync("Expensive Sword", 100, levelReq: 1);

            var command = new PurchaseItemCommand(item.Id, userAccount.Profile.Id);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<PurchaseItemException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Insufficient funds.", exception.Message);
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenInsufficientLevel()
        {
            // Arrange
            var userAccount = await CreateUserWithLevelAndCoins(5, 1000); // Level 5
            var item = await AddShopItemAsync("High Level Armor", 100, levelReq: 10); // Requires Level 10

            var command = new PurchaseItemCommand(item.Id, userAccount.Profile.Id);

            // Act & Assert
            // Assuming you use DomainException or ConflictException for this case
            var exception = await Assert.ThrowsAsync<PurchaseItemException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Insufficient level to purchase this item.", exception.Message);
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenBuyingDuplicateUniqueItem()
        {
            // Arrange
            var userAccount = await CreateUserWithLevelAndCoins(10, 1000);
            var item = await AddShopItemAsync("Unique Frame", 100, levelReq: 1, isUnique: true);

            // Already own it
            await AddUserInventoryItemAsync(userAccount.Profile.Id, item.Id, 1);

            var command = new PurchaseItemCommand(item.Id, userAccount.Profile.Id);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<PurchaseItemException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("You already own this item.", exception.Message);
        }

        // Helper to create ShopItem via reflection
        private async Task<ShopItem> AddShopItemAsync(string name, int price, int levelReq, bool isUnique = false, bool isPurchasable = true)
        {
            var item = (ShopItem)Activator.CreateInstance(typeof(ShopItem), true)!;

            SetPrivateProperty(item, "Name", name);
            SetPrivateProperty(item, "Price", price);
            SetPrivateProperty(item, "LevelRequirement", levelReq);
            SetPrivateProperty(item, "IsUnique", isUnique);
            SetPrivateProperty(item, "IsPurchasable", isPurchasable);
            SetPrivateProperty(item, "Description", "Desc");
            SetPrivateProperty(item, "ImageUrl", "url");
            SetPrivateProperty(item, "Category", ShopItemsCategoryEnum.Consumables);
            SetPrivateProperty(item, "ItemType", isUnique ? ShopItemTypeEnum.Cosmetic : ShopItemTypeEnum.Consumable);

            _context.ShopItems.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        private static void SetPrivateProperty(object obj, string propertyName, object value)
        {
            var prop = obj.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            prop?.SetValue(obj, value);
        }
    }
}
