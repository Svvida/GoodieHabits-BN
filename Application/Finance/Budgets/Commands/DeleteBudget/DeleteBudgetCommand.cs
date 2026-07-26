using Application.Common.Interfaces;

namespace Application.Finance.Budgets.Commands.DeleteBudget
{
    public record DeleteBudgetCommand(int BudgetId, int UserProfileId) : ICommand;
}
