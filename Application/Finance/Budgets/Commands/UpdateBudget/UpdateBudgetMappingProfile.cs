using Mapster;

namespace Application.Finance.Budgets.Commands.UpdateBudget
{
    public class UpdateBudgetMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // BudgetId (route) and UserProfileId (identity) are attached in the controller.
            config.NewConfig<UpdateBudgetRequest, UpdateBudgetCommand>();
        }
    }
}
