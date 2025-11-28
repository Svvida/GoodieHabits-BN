using Application.Accounts.Dtos;
using Application.Common.Interfaces;
using Domain.Enums;
using Domain.Models;
using Domain.ValueObjects;
using Mapster;

namespace Application.Accounts.Mappings
{
    public class GetAccountWithProfileMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // 1. Account -> Response
            config.NewConfig<Account, GetAccountWithProfileResponse>()
                .Map(dest => dest.JoinDate, src => src.CreatedAt)
                .Map(dest => dest.Preferences, src => src.Profile);

            // 2. UserProfile -> UserProfileInfoDto
            config.NewConfig<UserProfile, UserProfileInfoDto>()
                .Map(dest => dest.Badges, src => src.UserProfile_Badges)
                .Map(dest => dest.Avatar, src => MapContext.Current.GetService<IUrlBuilder>().BuildProfilePageAvatarUrl(src.Avatar));

            // 3. UserProfile_Badge -> BadgeDto
            config.NewConfig<UserProfile_Badge, BadgeDto>()
                .Map(dest => dest.Type, src => src.Badge.Type)
                .Map(dest => dest.Text, src => src.Badge.Text)
                .Map(dest => dest.ColorHex, src => src.Badge.ColorHex)
                .Map(dest => dest.Description, src => src.Badge.Description);

            // 4. UserProfile -> AccountPreferencesDto
            config.NewConfig<UserProfile, AccountPreferencesDto>()
                .Map(dest => dest.ActiveCosmetics, src => src);

            // 5. UserProfile -> ActiveCosmeticsDto
            config.NewConfig<UserProfile, ActiveCosmeticsDto>()
                .Map(dest => dest.AvatarFrameUrl, src => GetFrameUrl(src))
                .Map(dest => dest.Title, src => GetTitle(src))
                .Map(dest => dest.Pet, src => GetPet(src))
                .Map(dest => dest.NameEffect, src => GetNameEffect(src));
        }

        private static string? GetFrameUrl(UserProfile profile)
        {
            var item = profile.InventoryItems
                .FirstOrDefault(ii => ii.ShopItem.Category == ShopItemsCategoryEnum.AvatarFrames && ii.IsActive);

            if (item?.ShopItem.Payload is AvatarFramePayload)
                return MapContext.Current.GetService<IUrlBuilder>().BuildCosmeticUrl(item.ShopItem.ImageUrl);

            return null;
        }

        private static string? GetTitle(UserProfile profile)
        {
            var item = profile.InventoryItems
                .FirstOrDefault(ii => ii.ShopItem.Category == ShopItemsCategoryEnum.Titles && ii.IsActive);
            if (item?.ShopItem.Payload is TitlePayload titlePayload)
                return titlePayload.TitleText;
            return null;
        }

        private static PetDto? GetPet(UserProfile profile)
        {
            var item = profile.InventoryItems
                .FirstOrDefault(ii => ii.ShopItem.Category == ShopItemsCategoryEnum.Pets && ii.IsActive);

            if (item?.ShopItem.Payload is PetPayload payload)
            {
                var url = MapContext.Current.GetService<IUrlBuilder>()
                    .BuildPetUrl(item.ShopItem.ImageUrl);

                return new PetDto(url, payload.Animation);
            }

            return null;
        }

        private static NameEffectDto? GetNameEffect(UserProfile profile)
        {
            var item = profile.InventoryItems
                .FirstOrDefault(ii => ii.ShopItem.Category == ShopItemsCategoryEnum.NameEffects && ii.IsActive);
            if (item?.ShopItem.Payload is NameEffectPayload payload)
                return new NameEffectDto(payload.EffectStyle, payload.ColorHex);
            return null;
        }
    }
}
