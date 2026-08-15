using Mapster;

namespace Application.Supplements.Catalog.Commands.AddSupplementSlot
{
    public class AddSupplementSlotMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // SupplementId and UserProfileId are attached in the controller (route + identity).
            config.NewConfig<SupplementSlotRequest, AddSupplementSlotCommand>();
        }
    }
}
