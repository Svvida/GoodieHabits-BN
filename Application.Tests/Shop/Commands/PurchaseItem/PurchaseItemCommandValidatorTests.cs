using System.Reflection;
using Application.Shop.Commands.PurchaseItem;
using Domain.Enums;
using Domain.Models;
using FluentValidation.TestHelper;

namespace Application.Tests.Shop.Commands.PurchaseItem
{
    public class PurchaseItemCommandValidatorTests : TestBase<PurchaseItemCommandValidator>
    {
        private readonly PurchaseItemCommandValidator _validator;
        public PurchaseItemCommandValidatorTests()
        {
            _validator = new PurchaseItemCommandValidator(_unitOfWork);
        }
        [Fact]
        public async Task Validate_ShouldHaveError_WhenUserProfileIdIsInvalid()
        {
            var command = new PurchaseItemCommand(1, 0);
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.UserProfileId);
        }

        [Fact]
        public async Task Validate_ShouldHaveError_WhenUserProfileDoesNotExist()
        {
            var command = new PurchaseItemCommand(1, 999);
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.UserProfileId)
                  .WithErrorMessage("User Profile not found.");
        }

        [Fact]
        public async Task Validate_ShouldHaveError_WhenShopItemIdIsInvalid()
        {
            var command = new PurchaseItemCommand(0, 1);
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.ShopItemId);
        }

        [Fact]
        public async Task Validate_ShouldHaveError_WhenShopItemDoesNotExist()
        {
            var user = await AddAccountAsync("user@test.com", "pass", "user");
            var command = new PurchaseItemCommand(999, user.Profile.Id);

            var result = await _validator.TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ShopItemId)
                  .WithErrorMessage("Shop item not found or unavailable.");
        }

        [Fact]
        public async Task Validate_ShouldHaveError_WhenShopItemIsNotPurchasable()
        {
            var user = await AddAccountAsync("user@test.com", "pass", "user");
            var item = await AddShopItemAsync("Secret Item", 100, isPurchasable: false);

            var command = new PurchaseItemCommand(item.Id, user.Profile.Id);

            var result = await _validator.TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ShopItemId)
                  .WithErrorMessage("Shop item not found or unavailable.");
        }

        [Fact]
        public async Task Validate_ShouldPass_WhenRequestIsValid()
        {
            var user = await AddAccountAsync("user@test.com", "pass", "user");
            var item = await AddShopItemAsync("Sword", 100, isPurchasable: true);

            var command = new PurchaseItemCommand(item.Id, user.Profile.Id);

            var result = await _validator.TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // Helper to create ShopItem via reflection since constructor is protected/internal
        private async Task<ShopItem> AddShopItemAsync(string name, int price, bool isPurchasable = true)
        {
            var item = (ShopItem)Activator.CreateInstance(typeof(ShopItem), true)!;

            SetPrivateProperty(item, "Name", name);
            SetPrivateProperty(item, "Price", price);
            SetPrivateProperty(item, "IsPurchasable", isPurchasable);
            SetPrivateProperty(item, "Description", "Desc");
            SetPrivateProperty(item, "ImageUrl", "url");
            SetPrivateProperty(item, "Category", ShopItemsCategoryEnum.Consumables);
            SetPrivateProperty(item, "ItemType", ShopItemTypeEnum.Consumable);

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
