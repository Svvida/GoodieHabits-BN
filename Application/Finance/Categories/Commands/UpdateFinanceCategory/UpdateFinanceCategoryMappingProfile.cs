using Mapster;

namespace Application.Finance.Categories.Commands.UpdateFinanceCategory
{
    public class UpdateFinanceCategoryMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // CategoryId (route) and UserProfileId (identity) are attached in the controller.
            config.NewConfig<UpdateFinanceCategoryRequest, UpdateFinanceCategoryCommand>();
        }
    }
}
