using Application.Finance.Budgets.Dtos;
using Domain.Models;
using Mapster;

namespace Application.Finance.Budgets.Mappings
{
    public class BudgetMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Budget, BudgetDto>();
        }
    }
}
