using Mapster;

namespace Application.Supplements.Catalog.Commands.UpdateSupplement
{
    public class UpdateSupplementMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // SupplementId and UserProfileId are attached in the controller (route + identity).
            config.NewConfig<UpdateSupplementRequest, UpdateSupplementCommand>();
        }
    }
}
