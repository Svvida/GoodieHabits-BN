using Mapster;

namespace Application.Supplements.Catalog.Commands.CreateSupplement
{
    public class CreateSupplementMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // UserProfileId is attached from the authenticated identity in the controller.
            config.NewConfig<CreateSupplementRequest, CreateSupplementCommand>();
        }
    }
}
