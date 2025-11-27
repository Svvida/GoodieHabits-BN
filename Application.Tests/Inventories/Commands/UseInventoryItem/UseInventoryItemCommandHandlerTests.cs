using Application.Inventories.Commands.UseInventoryItem;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests.Inventories.Commands.UseInventoryItem
{
    public class UseInventoryItemCommandHandlerTests : TestBase<UseInventoryItemCommandHandler>
    {
        private readonly UseInventoryItemCommandHandler _handler;

        public UseInventoryItemCommandHandlerTests()
        {
            _handler = new UseInventoryItemCommandHandler(_unitOfWork, _clockMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldApplyEffectAndDecrementQuantity_WhenMultipleItemsExist()
        {
            // Arrange
            var user = await CreateUserWithLevelAndCoins(1, 0);

            // Create Consumable: +50 XP, Lasts 30 mins
            var payload = new ConsumablePayload
            {
                EffectType = ConsumablesEffectTypeEnum.XpMultiplier,
                Multiplier = 1.5,
                DurationMinutes = 30
            };

            var potion = await GetOrCreateShopItemAsync(9100, "XP Potion", ShopItemsCategoryEnum.Consumables, ShopItemTypeEnum.Consumable, payload: payload);

            // Give user 5 potions
            var invItem = await AddUserInventoryItemAsync(user.Profile.Id, potion.Id, 5);

            var command = new UseInventoryItemCommand(user.Profile.Id, invItem.Id);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            // 1. Check Inventory Quantity
            var dbItem = await _context.UserInventories.FindAsync(invItem.Id);
            dbItem.Should().NotBeNull();
            dbItem!.Quantity.Should().Be(4); // 5 - 1 = 4

            // 2. Check Active Effect Created
            var activeEffect = await _context.ActiveUserEffects.FirstOrDefaultAsync(e => e.UserProfileId == user.Profile.Id);
            activeEffect.Should().NotBeNull();
            activeEffect!.SourceItemId.Should().Be(potion.Id);
            activeEffect.EffectType.Should().Be(ConsumablesEffectTypeEnum.XpMultiplier);

            // 3. Verify Expiration Time (Using Fixed Clock from TestBase)
            // _fixedTestInstant is 2023-10-26 10:00:00 UTC
            var expectedExpiry = _fixedTestInstant.ToDateTimeUtc().AddMinutes(30);
            activeEffect.ExpiresAt.Should().Be(expectedExpiry);
        }

        [Fact]
        public async Task Handle_ShouldDeleteInventoryRow_WhenQuantityReachesZero()
        {
            // Arrange
            var user = await CreateUserWithLevelAndCoins(1, 0);

            var payload = new ConsumablePayload { EffectType = ConsumablesEffectTypeEnum.NextQuestXpBonus, FlatValue = 100 };
            var snack = await GetOrCreateShopItemAsync(9101, "Snack", ShopItemsCategoryEnum.Consumables, ShopItemTypeEnum.Consumable, payload: payload);

            // Give user EXACTLY 1 item
            var invItem = await AddUserInventoryItemAsync(user.Profile.Id, snack.Id, 1);

            var command = new UseInventoryItemCommand(user.Profile.Id, invItem.Id);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            // The item should be REMOVED from the database
            var dbItem = await _context.UserInventories.FindAsync(invItem.Id);
            dbItem.Should().BeNull("Row should be deleted when quantity hits 0");

            // Effect should still exist
            var activeEffect = await _context.ActiveUserEffects.FirstOrDefaultAsync(e => e.UserProfileId == user.Profile.Id);
            activeEffect.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ShouldThrowConflict_WhenItemIsNotConsumable()
        {
            // Arrange
            var user = await CreateUserWithLevelAndCoins(1, 0);

            // Try to eat a Sword
            var sword = await GetOrCreateShopItemAsync(9102, "Tasty Sword", ShopItemsCategoryEnum.Avatars, ShopItemTypeEnum.Cosmetic);
            var invItem = await AddUserInventoryItemAsync(user.Profile.Id, sword.Id, 1);

            var command = new UseInventoryItemCommand(user.Profile.Id, invItem.Id);

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowInvalidArgument_WhenPayloadIsInvalid()
        {
            // This tests the Domain Logic "Pattern Matching" guard clause
            // Arrange
            var user = await CreateUserWithLevelAndCoins(1, 0);

            // Create an item categorized as Consumable, but with WRONG payload (e.g. PetPayload)
            // This simulates bad data configuration
            var brokenItem = await GetOrCreateShopItemAsync(9103, "Bugged Item", ShopItemsCategoryEnum.Consumables, ShopItemTypeEnum.Consumable, payload: new PetPayload { PetId = "dog" });

            var invItem = await AddUserInventoryItemAsync(user.Profile.Id, brokenItem.Id, 1);

            var command = new UseInventoryItemCommand(user.Profile.Id, invItem.Id);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidArgumentException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }
    }
}
