namespace Application.Finance.Budgets.Commands.UpdateBudget
{
    // Only the limit is mutable; scope (category) and period identify the budget and are immutable.
    public record UpdateBudgetRequest(decimal LimitAmount);
}
