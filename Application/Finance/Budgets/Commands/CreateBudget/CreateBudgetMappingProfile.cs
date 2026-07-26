using Mapster;

namespace Application.Finance.Budgets.Commands.CreateBudget
{
    public class CreateBudgetMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // UserProfileId is attached from the authenticated identity in the controller.
            config.NewConfig<CreateBudgetRequest, CreateBudgetCommand>();
        }
    }
}
