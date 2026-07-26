using Application.Finance.Analytics.Common;
using Application.Finance.Analytics.Dtos;
using Domain.Calculators;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;

namespace Application.Finance.Analytics.Queries.GetYearlySummary
{
    public class GetYearlySummaryQueryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetYearlySummaryQuery, YearlySummaryDto>
    {
        public async Task<YearlySummaryDto> Handle(GetYearlySummaryQuery request, CancellationToken cancellationToken)
        {
            var (start, end) = FinancePeriodCalculator.ForYear(request.Year);

            var transactions = await unitOfWork.FinanceTransactions
                .GetForPeriodAsync(request.UserProfileId, start, end, null, cancellationToken).ConfigureAwait(false);

            var categories = await unitOfWork.FinanceCategories
                .GetUserCategoryTreeAsync(request.UserProfileId, true, cancellationToken).ConfigureAwait(false);
            var categoriesById = categories.ToDictionary(c => c.Id);

            var profile = await unitOfWork.UserProfiles.GetByIdAsync(request.UserProfileId, cancellationToken).ConfigureAwait(false);
            var currency = profile?.Currency ?? "USD";

            var income = transactions.Where(t => t.Type == FinanceTransactionTypeEnum.Income).ToList();
            var expense = transactions.Where(t => t.Type == FinanceTransactionTypeEnum.Expense).ToList();

            var months = new List<MonthlyTotalsDto>(12);
            for (var month = 1; month <= 12; month++)
            {
                var monthIncome = income.Where(t => t.OccurredOn.Month == month).Sum(t => t.Amount);
                var monthExpense = expense.Where(t => t.OccurredOn.Month == month).Sum(t => t.Amount);
                months.Add(new MonthlyTotalsDto
                {
                    Month = month,
                    TotalIncome = monthIncome,
                    TotalExpense = monthExpense,
                    Net = monthIncome - monthExpense,
                });
            }

            var totalIncome = income.Sum(t => t.Amount);
            var totalExpense = expense.Sum(t => t.Amount);

            return new YearlySummaryDto
            {
                Year = request.Year,
                Currency = currency,
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                Net = totalIncome - totalExpense,
                Months = months,
                ExpenseByCategory = FinanceAnalyticsHelper.BuildBreakdown(expense, categoriesById),
                IncomeByCategory = FinanceAnalyticsHelper.BuildBreakdown(income, categoriesById),
            };
        }
    }
}
