using Application.Inventories.Queries.GetUserInventoryItems;
using Domain.Enums;
using FluentAssertions;

namespace Application.Tests.Inventories.Queries.GetUserInventoryItems
{
    public class GetUserInventoryItemsQueryHandlerTests : TestBase<GetUserInventoryItemsQueryHandler>
    {
        private readonly GetUserInventoryItemsQueryHandler _handler;

        public GetUserInventoryItemsQueryHandlerTests()
        {
            _handler = new GetUserInventoryItemsQueryHandler(_unitOfWork, _mapper);
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectItems_WhenUsingSeedData()
        {
            // Arrange
            var user = await CreateUserWithLevelAndCoins(10, 1000);

            await AddUserInventoryItemAsync(user.Profile.Id, shopItemId: 200, quantity: 1);
            await AddUserInventoryItemAsync(user.Profile.Id, shopItemId: 600, quantity: 5);

            var query = new GetUserInventoryItemsQuery(user.Profile.Id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);

            // Verify the Cat Programmer (Seed Data Verification)
            var avatar = result.Single(x => x.ShopItemId == 200);
            avatar.ItemName.Should().Be("Cat Programmer");
            avatar.Category.Should().Be(ShopItemsCategoryEnum.Avatars.ToString());
            avatar.ItemUrl.Should().Contain("cat_programmer");

            // Verify the Consumable
            var snack = result.Single(x => x.ShopItemId == 600);
            snack.ItemName.Should().Be("Mini XP Snack");
            snack.Quantity.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ShouldHandleCustomTestItems_WithSafeIds()
        {
            // Arrange
            var user = await CreateUserWithLevelAndCoins(5, 500);

            // CREATE NEW DATA using a "Safe ID" (e.g., 9000+) to ensure no conflict with seed data
            var customItem = await GetOrCreateShopItemAsync(
                id: 9999,
                name: "Integration Test Sword",
                category: ShopItemsCategoryEnum.Avatars,
                type: ShopItemTypeEnum.Cosmetic
            );

            await AddUserInventoryItemAsync(user.Profile.Id, customItem.Id, quantity: 1);

            var query = new GetUserInventoryItemsQuery(user.Profile.Id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
            result.First().ItemName.Should().Be("Integration Test Sword");
        }
    }
}
