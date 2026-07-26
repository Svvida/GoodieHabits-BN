using Application.Finance.Analytics.Dtos;
using Domain.Calculators;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;

namespace Application.Finance.Analytics.Queries.GetBudgetProgress
{
    public class GetBudgetProgressQueryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetBudgetProgressQuery, IEnumerable<BudgetProgressItemDto>>
    {
        public async Task<IEnumerable<BudgetProgressItemDto>> Handle(GetBudgetProgressQuery request, CancellationToken cancellationToken)
        {
            var budgets = await unitOfWork.Budgets
                .GetUserBudgetsAsync(request.UserProfileId, request.Year, request.Month, true, cancellationToken).ConfigureAwait(false);

            if (budgets.Count == 0)
                return [];

            // Budgets track spending, so only expenses count. Fetch the whole year once and slice per budget.
            var (yearStart, yearEnd) = FinancePeriodCalculator.ForYear(request.Year);
            var expenses = await unitOfWork.FinanceTransactions
                .GetForPeriodAsync(request.UserProfileId, yearStart, yearEnd, FinanceTransactionTypeEnum.Expense, cancellationToken).ConfigureAwait(false);

            var categories = await unitOfWork.FinanceCategories
                .GetUserCategoryTreeAsync(request.UserProfileId, true, cancellationToken).ConfigureAwait(false);
            var categoriesById = categories.ToDictionary(c => c.Id);

            var result = new List<BudgetProgressItemDto>(budgets.Count);
            foreach (var budget in budgets)
            {
                var (start, end) = FinancePeriodCalculator.ForPeriod(budget.Period, budget.Year, budget.Month);

                var spent = expenses
                    .Where(t => t.OccurredOn >= start && t.OccurredOn <= end)
                    .Where(t => budget.CategoryId == null || t.CategoryId == budget.CategoryId)
                    .Sum(t => t.Amount);

                var progress = BudgetProgressCalculator.Calculate(budget.LimitAmount, spent);

                result.Add(new BudgetProgressItemDto
                {
                    BudgetId = budget.Id,
                    CategoryId = budget.CategoryId,
                    CategoryName = budget.CategoryId is int id && categoriesById.TryGetValue(id, out var category) ? category.Name : null,
                    Period = budget.Period,
                    Year = budget.Year,
                    Month = budget.Month,
                    Limit = progress.Limit,
                    Spent = progress.Spent,
                    Remaining = progress.Remaining,
                    PercentUsed = progress.PercentUsed,
                    IsOverBudget = progress.IsOverBudget,
                });
            }

            return result;
        }
    }
}
