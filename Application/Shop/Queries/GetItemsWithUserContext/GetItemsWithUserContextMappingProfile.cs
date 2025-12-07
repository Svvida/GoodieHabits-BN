using Domain.Models;
using Mapster;

namespace Application.Shop.Queries.GetItemsWithUserContext
{
    public class GetItemsWithUserContextMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<ShopItem, ShopItemDto>()
                .Map(dest => dest.ItemType, src => src.ItemType.ToString())
                .Map(dest => dest.Category, src => src.Category.ToString())
                .Map(dest => dest.CurrencyType, src => src.CurrencyType.ToString());
        }
    }
}
