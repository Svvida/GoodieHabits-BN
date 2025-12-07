using Application.Accounts.Queries.GetWithProfile;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.Tests.Accounts.Queries.GetWithProfile
{
    public class GetAccountWithProfileQueryHandlerTests : TestBase<GetAccountWithProfileQueryHandler>
    {
        private readonly GetAccountWithProfileQueryHandler _handler;

        public GetAccountWithProfileQueryHandlerTests()
        {
            _urlBuilderMock.Setup(b => b.BuildCosmeticUrl(It.IsAny<string>()))
                .Returns((string publicId) => $"mock_cosmetic_{publicId}");

            _urlBuilderMock.Setup(b => b.BuildPetUrl(It.IsAny<string>()))
               .Returns((string publicId) => $"mock_pet_{publicId}");

            _handler = new GetAccountWithProfileQueryHandler(_unitOfWork, _mapper);
        }

        [Fact]
        public async Task Handle_ShouldReturnProfileWithActiveCosmetics_WhenItemsAreEquipped()
        {
            // Arrange
            var user = await CreateUserWithLevelAndCoins(10, 1000);

            // -----------------------------------------------------------
            // 2. CREATE SHOP ITEMS
            // -----------------------------------------------------------

            // Frame: Logic uses ShopItem.ImageUrl for the URL
            var frameItem = await GetOrCreateShopItemAsync(9501, "Gold Frame",
                ShopItemsCategoryEnum.AvatarFrames, ShopItemTypeEnum.Cosmetic,
                payload: new AvatarFramePayload { FrameId = "gold_1" });

            // Pet: Logic uses ShopItem.ImageUrl for URL, Payload for Animation
            var petItem = await GetOrCreateShopItemAsync(9502, "Dragon",
                ShopItemsCategoryEnum.Pets, ShopItemTypeEnum.Pet,
                payload: new PetPayload { PetId = "dragon", Animation = "fly" });

            // Title: Logic uses Payload.TitleText directly
            var titleItem = await GetOrCreateShopItemAsync(9503, "The Boss",
                ShopItemsCategoryEnum.Titles, ShopItemTypeEnum.Title,
                payload: new TitlePayload { TitleText = "The Boss" });

            // Name Effect: Logic uses Payload.EffectStyle
            var effectItem = await GetOrCreateShopItemAsync(9504, "Neon",
                ShopItemsCategoryEnum.NameEffects, ShopItemTypeEnum.VisualEffect,
                payload: new NameEffectPayload { EffectStyle = "neon_blue", ColorHex = "#0000FF" });


            // -----------------------------------------------------------
            // 3. ADD TO INVENTORY & EQUIP
            // -----------------------------------------------------------
            var invFrame = await AddUserInventoryItemAsync(user.Profile.Id, frameItem.Id, 1);
            var invPet = await AddUserInventoryItemAsync(user.Profile.Id, petItem.Id, 1);
            var invTitle = await AddUserInventoryItemAsync(user.Profile.Id, titleItem.Id, 1);
            var invEffect = await AddUserInventoryItemAsync(user.Profile.Id, effectItem.Id, 1);

            // Manually set them to Active (mimicking the Equip command)
            invFrame.Equip();
            invPet.Equip();
            invTitle.Equip();
            invEffect.Equip();
            await _unitOfWork.SaveChangesAsync();

            var query = new GetAccountWithProfileQuery(user.Id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be(user.Email);

            // Verify Preferences -> ActiveCosmetics
            var cosmetics = result.Preferences.ActiveCosmetics;
            cosmetics.Should().NotBeNull();

            // 1. Verify Frame URL (Should use BuildCosmeticUrl mock)
            // The helper 'GetOrCreateShopItemAsync' generates ImageUrl like "images/gold_frame"
            cosmetics.AvatarFrameUrl.Should().StartWith("mock_cosmetic_");
            cosmetics.AvatarFrameUrl.Should().Contain("gold_frame");

            // 2. Verify Pet (Should use BuildPetUrl mock + Payload data)
            cosmetics.Pet.Should().NotBeNull();
            cosmetics.Pet!.PetUrl.Should().StartWith("mock_pet_");
            cosmetics.Pet!.PetUrl.Should().Contain("dragon");
            cosmetics.Pet!.Animation.Should().Be("fly");

            // 3. Verify Title (Should come directly from Payload)
            cosmetics.Title.Should().Be("The Boss");

            // 4. Verify Name Effect
            cosmetics.NameEffect.Should().NotBeNull();
            cosmetics.NameEffect!.EffectStyle.Should().Be("neon_blue");
            cosmetics.NameEffect!.ColorHex.Should().Be("#0000FF");
        }

        [Fact]
        public async Task Handle_ShouldReturnNulls_WhenUserHasNoActiveCosmetics()
        {
            // Arrange
            var user = await CreateUserWithLevelAndCoins(1, 0);

            // Add an item but DO NOT equip it
            var inactiveItem = await GetOrCreateShopItemAsync(9600, "Inactive Frame", ShopItemsCategoryEnum.AvatarFrames, ShopItemTypeEnum.Cosmetic);
            await AddUserInventoryItemAsync(user.Profile.Id, inactiveItem.Id, 1);

            var query = new GetAccountWithProfileQuery(user.Id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            var cosmetics = result.Preferences.ActiveCosmetics;

            // Should be present (not null object), but properties should be null
            cosmetics.Should().NotBeNull();
            cosmetics.AvatarFrameUrl.Should().BeNull();
            cosmetics.Pet.Should().BeNull();
            cosmetics.Title.Should().BeNull();
            cosmetics.NameEffect.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFound_WhenAccountDoesNotExist()
        {
            // Arrange
            var query = new GetAccountWithProfileQuery(99999); // Invalid ID

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldCorrectlyMapBasicProfileInfo()
        {
            // Arrange
            var user = await CreateUserWithLevelAndCoins(5, 100);
            var query = new GetAccountWithProfileQuery(user.Id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Profile.Nickname.Should().Be(user.Profile.Nickname);
            // Verify UploadedAvatarUrl Mock
            // user.Profile.UploadedAvatarUrl is usually null in new accounts, check logic
            if (string.IsNullOrEmpty(user.Profile.UploadedAvatarUrl))
            {
                result.Profile.Avatar.Should().BeEmpty();
            }
            else
            {
                result.Profile.Avatar.Should().Contain("mock_url");
            }
        }
    }
}
