using Application.Finance.Categories.Dtos;
using Domain.Models;
using Mapster;

namespace Application.Finance.Categories.Mappings
{
    public class FinanceCategoryMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // SubCategories are assembled explicitly by the tree query, not pulled from the (unloaded) navigation.
            config.NewConfig<FinanceCategory, FinanceCategoryDto>()
                .Ignore(dest => dest.SubCategories);
        }
    }
}
