using Application.Supplements.Catalog.Commands.AddSupplementSlot;
using Mapster;

namespace Application.Supplements.Catalog.Commands.UpdateSupplementSlot
{
    public class UpdateSupplementSlotMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // Add and update share one request shape; the ids come from the route and the identity.
            config.NewConfig<SupplementSlotRequest, UpdateSupplementSlotCommand>();
        }
    }
}
