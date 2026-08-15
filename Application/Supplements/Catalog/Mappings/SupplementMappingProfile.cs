using Application.Supplements.Catalog.Dtos;
using Domain.Models;
using Mapster;

namespace Application.Supplements.Catalog.Mappings
{
    public class SupplementMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<SupplementScheduleSlot, SupplementSlotDto>();
            config.NewConfig<Supplement, SupplementDto>();
        }
    }
}
