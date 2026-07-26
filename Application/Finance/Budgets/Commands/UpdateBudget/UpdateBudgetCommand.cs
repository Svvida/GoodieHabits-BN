using Application.Common.Interfaces;
using Application.Finance.Budgets.Dtos;

namespace Application.Finance.Budgets.Commands.UpdateBudget
{
    public record UpdateBudgetCommand(int BudgetId, decimal LimitAmount, int UserProfileId) : ICommand<BudgetDto>;
}
