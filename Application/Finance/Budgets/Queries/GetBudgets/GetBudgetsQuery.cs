using Application.Common.Interfaces;
using Application.Finance.Budgets.Dtos;

namespace Application.Finance.Budgets.Queries.GetBudgets
{
    // Month is optional: when set, returns that month's monthly budgets plus the year's yearly budgets.
    public record GetBudgetsQuery(int UserProfileId, int Year, int? Month) : IQuery<IEnumerable<BudgetDto>>;
}
