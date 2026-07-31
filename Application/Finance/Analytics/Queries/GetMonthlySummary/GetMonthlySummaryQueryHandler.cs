using Application.Finance.Analytics.Common;
using Application.Finance.Analytics.Dtos;
using Domain.Calculators;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;

namespace Application.Finance.Analytics.Queries.GetMonthlySummary
{
    public class GetMonthlySummaryQueryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetMonthlySummaryQuery, MonthlySummaryDto>
    {
        public async Task<MonthlySummaryDto> Handle(GetMonthlySummaryQuery request, CancellationToken cancellationToken)
        {
            var (start, end) = FinancePeriodCalculator.ForMonth(request.Year, request.Month);

            var transactions = await unitOfWork.FinanceTransactions
                .GetForPeriodAsync(request.UserProfileId, start, end, null, cancellationToken).ConfigureAwait(false);

            var categories = await unitOfWork.FinanceCategories
                .GetUserCategoryTreeAsync(request.UserProfileId, true, cancellationToken).ConfigureAwait(false);
            var categoriesById = categories.ToDictionary(c => c.Id);

            var currency = await GetCurrencyAsync(request.UserProfileId, cancellationToken).ConfigureAwait(false);

            var income = transactions.Where(t => t.Type == FinanceTransactionTypeEnum.Income).ToList();
            var expense = transactions.Where(t => t.Type == FinanceTransactionTypeEnum.Expense).ToList();

            var totalIncome = income.Sum(t => t.NetAmount);
            var totalExpense = expense.Sum(t => t.NetAmount);

            return new MonthlySummaryDto
            {
                Year = request.Year,
                Month = request.Month,
                Currency = currency,
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                Net = totalIncome - totalExpense,
                ExpenseByCategory = FinanceAnalyticsHelper.BuildBreakdown(expense, categoriesById),
                IncomeByCategory = FinanceAnalyticsHelper.BuildBreakdown(income, categoriesById),
            };
        }

        private async Task<string> GetCurrencyAsync(int userProfileId, CancellationToken cancellationToken)
        {
            var profile = await unitOfWork.UserProfiles.GetByIdAsync(userProfileId, cancellationToken).ConfigureAwait(false);
            return profile?.Currency ?? "USD";
        }
    }
}
