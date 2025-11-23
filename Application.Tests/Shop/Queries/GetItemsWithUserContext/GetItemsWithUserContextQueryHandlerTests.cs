using Application.Common.Sorting;
using Application.Shop.Queries.GetItemsWithUserContext;
using Domain.Enums;
using FluentAssertions;

namespace Application.Tests.Shop.Queries.GetItemsWithUserContext
{
    public class GetItemsWithUserContextQueryHandlerTests : TestBase<GetItemsWithUserContextQueryHandler>
    {
        private readonly GetItemsWithUserContextQueryHandler _handler;

        public GetItemsWithUserContextQueryHandlerTests()
        {
            _handler = new GetItemsWithUserContextQueryHandler(_unitOfWork, _levelCalculatorMock.Object, _mapper);
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectUserContext_WhenItemIsPurchasable()
        {
            // Arrange
            var user = await CreateUserWithLevelAndCoins(level: 5, coins: 500);
            // Item 200: Cat Programmer, Price 300, Level 1
            var query = new GetItemsWithUserContextQuery(user.Profile.Id, ShopItemsCategoryEnum.Avatars);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            var catProgrammerItem = result.FirstOrDefault(i => i.Id == 200);
            catProgrammerItem.Should().NotBeNull();
            catProgrammerItem!.UserContext.Should().NotBeNull();
            catProgrammerItem.UserContext.CanPurchase.Should().BeTrue();
            catProgrammerItem.UserContext.IsOwned.Should().BeFalse();
            catProgrammerItem.UserContext.IsUnlocked.Should().BeTrue();
            catProgrammerItem.UserContext.CanAfford.Should().BeTrue();
            catProgrammerItem.UserContext.PurchaseLockReason.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectUserContext_WhenLevelIsInsufficient()
        {
            // Arrange
            var user = await CreateUserWithLevelAndCoins(level: 1, coins: 1000);
            // Item 201: Giga-Chad Avatar, Price 400, Level 5
            var query = new GetItemsWithUserContextQuery(user.Profile.Id, ShopItemsCategoryEnum.Avatars);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            var gigaChadItem = result.FirstOrDefault(i => i.Id == 201);
            gigaChadItem.Should().NotBeNull();
            gigaChadItem!.UserContext.CanPurchase.Should().BeFalse();
            gigaChadItem.UserContext.IsUnlocked.Should().BeFalse();
            gigaChadItem.UserContext.PurchaseLockReason.Should().Be(PurchaseLockReasonEnum.InsufficientLevel.ToString());
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectUserContext_WhenFundsAreInsufficient()
        {
            // Arrange
            var user = await CreateUserWithLevelAndCoins(level: 10, coins: 100);
            // Item 201: Giga-Chad Avatar, Price 400, Level 5
            var query = new GetItemsWithUserContextQuery(user.Profile.Id, ShopItemsCategoryEnum.Avatars);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            var gigaChadItem = result.FirstOrDefault(i => i.Id == 201);
            gigaChadItem.Should().NotBeNull();
            gigaChadItem!.UserContext.CanPurchase.Should().BeFalse();
            gigaChadItem.UserContext.CanAfford.Should().BeFalse();
            gigaChadItem.UserContext.PurchaseLockReason.Should().Be(PurchaseLockReasonEnum.InsufficientFunds.ToString());
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectUserContext_WhenUniqueItemIsAlreadyOwned()
        {
            // Arrange
            var user = await CreateUserWithLevelAndCoins(level: 10, coins: 1000);
            // Item 201: Giga-Chad Avatar (Unique)
            await AddUserInventoryItemAsync(user.Profile.Id, 201, 1);
            var query = new GetItemsWithUserContextQuery(user.Profile.Id, ShopItemsCategoryEnum.Avatars);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            var gigaChadItem = result.FirstOrDefault(i => i.Id == 201);
            gigaChadItem.Should().NotBeNull();
            gigaChadItem!.UserContext.IsOwned.Should().BeTrue();
            gigaChadItem.UserContext.CanPurchase.Should().BeFalse();
            gigaChadItem.UserContext.PurchaseLockReason.Should().Be(PurchaseLockReasonEnum.AlreadyOwned.ToString());
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectUserContext_WhenConsumableIsOwned()
        {
            // Arrange
            var user = await CreateUserWithLevelAndCoins(level: 10, coins: 1000);
            // Item 600: Mini XP Snack (Not Unique)
            await AddUserInventoryItemAsync(user.Profile.Id, 600, 5);
            var query = new GetItemsWithUserContextQuery(user.Profile.Id, ShopItemsCategoryEnum.Consumables);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            var snackItem = result.FirstOrDefault(i => i.Id == 600);
            snackItem.Should().NotBeNull();
            snackItem!.UserContext.IsOwned.Should().BeTrue();
            snackItem.UserContext.QuantityOwned.Should().Be(5);
            snackItem.UserContext.CanPurchase.Should().BeTrue(); // Can still buy more
            snackItem.UserContext.PurchaseLockReason.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ShouldFilterByCategory()
        {
            // Arrange
            var user = await CreateUserWithLevelAndCoins(level: 1, coins: 1);
            var query = new GetItemsWithUserContextQuery(user.Profile.Id, ShopItemsCategoryEnum.Pets);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeEmpty();
            result.Should().OnlyContain(i => i.Category == ShopItemsCategoryEnum.Pets.ToString());
        }

        [Theory]
        [InlineData(ShopItemSortProperty.Name, SortOrder.Asc)]
        [InlineData(ShopItemSortProperty.Name, SortOrder.Desc)]
        [InlineData(ShopItemSortProperty.Price, SortOrder.Asc)]
        [InlineData(ShopItemSortProperty.Price, SortOrder.Desc)]
        public async Task Handle_ShouldSortItemsCorrectly(ShopItemSortProperty sortBy, SortOrder sortOrder)
        {
            // Arrange
            var user = await CreateUserWithLevelAndCoins(level: 1, coins: 1);
            var query = new GetItemsWithUserContextQuery(user.Profile.Id, null, sortBy, sortOrder);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeEmpty();
            switch (sortBy)
            {
                case ShopItemSortProperty.Name:
                    if (sortOrder == SortOrder.Asc)
                        result.Should().BeInAscendingOrder(i => i.Name);
                    else
                        result.Should().BeInDescendingOrder(i => i.Name);
                    break;
                case ShopItemSortProperty.Price:
                    if (sortOrder == SortOrder.Asc)
                        result.Should().BeInAscendingOrder(i => i.Price);
                    else
                        result.Should().BeInDescendingOrder(i => i.Price);
                    break;
            }
        }
    }
}
