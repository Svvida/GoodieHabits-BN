using Application.Finance.Analytics.Common;
using Application.Finance.Analytics.Dtos;
using Domain.Calculators;
using Domain.Interfaces;
using MediatR;

namespace Application.Finance.Analytics.Queries.GetCategoryBreakdown
{
    public class GetCategoryBreakdownQueryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetCategoryBreakdownQuery, CategoryBreakdownDto>
    {
        public async Task<CategoryBreakdownDto> Handle(GetCategoryBreakdownQuery request, CancellationToken cancellationToken)
        {
            var (start, end) = FinancePeriodCalculator.ForPeriod(request.Period, request.Year, request.Month);

            var transactions = await unitOfWork.FinanceTransactions
                .GetForPeriodAsync(request.UserProfileId, start, end, request.Type, cancellationToken).ConfigureAwait(false);

            var categories = await unitOfWork.FinanceCategories
                .GetUserCategoryTreeAsync(request.UserProfileId, true, cancellationToken).ConfigureAwait(false);
            var categoriesById = categories.ToDictionary(c => c.Id);

            var profile = await unitOfWork.UserProfiles.GetByIdAsync(request.UserProfileId, cancellationToken).ConfigureAwait(false);

            return new CategoryBreakdownDto
            {
                Type = request.Type,
                Year = request.Year,
                Month = request.Month,
                Currency = profile?.Currency ?? "USD",
                Total = transactions.Sum(t => t.NetAmount),
                Items = FinanceAnalyticsHelper.BuildBreakdown(transactions, categoriesById),
            };
        }
    }
}
