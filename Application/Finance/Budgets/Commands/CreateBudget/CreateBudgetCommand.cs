using Application.Common.Interfaces;
using Application.Finance.Budgets.Dtos;
using Domain.Enums;

namespace Application.Finance.Budgets.Commands.CreateBudget
{
    public record CreateBudgetCommand(
        int? CategoryId,
        BudgetPeriodEnum Period,
        int Year,
        int? Month,
        decimal LimitAmount,
        int UserProfileId) : ICommand<BudgetDto>;
}
