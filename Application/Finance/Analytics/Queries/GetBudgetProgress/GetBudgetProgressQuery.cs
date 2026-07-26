using Application.Common.Interfaces;
using Application.Finance.Analytics.Dtos;

namespace Application.Finance.Analytics.Queries.GetBudgetProgress
{
    // Month optional: with a month, evaluates that month's monthly budgets plus the year's yearly budgets.
    public record GetBudgetProgressQuery(int UserProfileId, int Year, int? Month)
        : IQuery<IEnumerable<BudgetProgressItemDto>>;
}
