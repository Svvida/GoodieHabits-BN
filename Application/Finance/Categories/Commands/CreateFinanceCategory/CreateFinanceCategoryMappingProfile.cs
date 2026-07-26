using Mapster;

namespace Application.Finance.Categories.Commands.CreateFinanceCategory
{
    public class CreateFinanceCategoryMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // UserProfileId is attached from the authenticated identity in the controller.
            config.NewConfig<CreateFinanceCategoryRequest, CreateFinanceCategoryCommand>();
        }
    }
}
