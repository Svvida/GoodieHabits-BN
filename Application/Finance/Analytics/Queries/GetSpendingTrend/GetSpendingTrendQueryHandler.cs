using Application.Finance.Analytics.Dtos;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;

namespace Application.Finance.Analytics.Queries.GetSpendingTrend
{
    public class GetSpendingTrendQueryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetSpendingTrendQuery, SpendingTrendDto>
    {
        public async Task<SpendingTrendDto> Handle(GetSpendingTrendQuery request, CancellationToken cancellationToken)
        {
            // First day of the window's starting month .. last day of the ending month.
            var firstOfEndMonth = new DateOnly(request.EndYear, request.EndMonth, 1);
            var windowStart = firstOfEndMonth.AddMonths(-(request.Months - 1));
            var windowEnd = firstOfEndMonth.AddMonths(1).AddDays(-1);

            var transactions = await unitOfWork.FinanceTransactions
                .GetForPeriodAsync(request.UserProfileId, windowStart, windowEnd, null, cancellationToken).ConfigureAwait(false);

            var profile = await unitOfWork.UserProfiles.GetByIdAsync(request.UserProfileId, cancellationToken).ConfigureAwait(false);

            var points = new List<SpendingTrendPointDto>(request.Months);
            for (var i = 0; i < request.Months; i++)
            {
                var monthDate = windowStart.AddMonths(i);
                var monthTxns = transactions
                    .Where(t => t.OccurredOn.Year == monthDate.Year && t.OccurredOn.Month == monthDate.Month)
                    .ToList();

                var income = monthTxns.Where(t => t.Type == FinanceTransactionTypeEnum.Income).Sum(t => t.NetAmount);
                var expense = monthTxns.Where(t => t.Type == FinanceTransactionTypeEnum.Expense).Sum(t => t.NetAmount);

                points.Add(new SpendingTrendPointDto
                {
                    Year = monthDate.Year,
                    Month = monthDate.Month,
                    Income = income,
                    Expense = expense,
                    Net = income - expense,
                });
            }

            return new SpendingTrendDto
            {
                Currency = profile?.Currency ?? "USD",
                Points = points,
            };
        }
    }
}
