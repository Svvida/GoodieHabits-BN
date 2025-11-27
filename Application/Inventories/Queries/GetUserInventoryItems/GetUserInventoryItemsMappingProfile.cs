using Application.Common.Interfaces;
using Domain.Models;
using Mapster;

namespace Application.Inventories.Queries.GetUserInventoryItems
{
    public class GetUserInventoryItemsMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UserInventory, UserInventoryItemDto>()
                .Map(dest => dest.UserInventoryId, src => src.Id)
                .Map(dest => dest.ShopItemId, src => src.ShopItemId)
                .Map(dest => dest.ItemType, src => src.ShopItem.ItemType.ToString())
                .Map(dest => dest.Category, src => src.ShopItem.Category.ToString())
                .Map(dest => dest.ItemUrl, src => MapContext.Current.GetService<IUrlBuilder>().BuildThumbnailAvatarUrl(src.ShopItem.ImageUrl))
                .Map(dest => dest.ItemName, src => src.ShopItem.Name)
                .Map(dest => dest.ItemDescription, src => src.ShopItem.Description)
                .Map(dest => dest.Quantity, src => src.Quantity)
                .Map(dest => dest.AcquiredAt, src => src.AcquiredAt)
                .Map(dest => dest.IsActive, src => src.IsActive);
        }
    }
}
