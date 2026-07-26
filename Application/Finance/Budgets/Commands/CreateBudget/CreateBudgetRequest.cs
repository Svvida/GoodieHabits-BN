using Domain.Enums;

namespace Application.Finance.Budgets.Commands.CreateBudget
{
    // CategoryId null => overall budget for the period. Month is required for Monthly, omitted for Yearly.
    public record CreateBudgetRequest(
        int? CategoryId,
        BudgetPeriodEnum Period,
        int Year,
        int? Month,
        decimal LimitAmount);
}
