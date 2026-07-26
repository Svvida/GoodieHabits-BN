using Application.Common.Interfaces;
using Application.Finance.Analytics.Dtos;
using Domain.Enums;

namespace Application.Finance.Analytics.Queries.GetCategoryBreakdown
{
    public record GetCategoryBreakdownQuery(
        int UserProfileId,
        FinanceTransactionTypeEnum Type,
        BudgetPeriodEnum Period,
        int Year,
        int? Month) : IQuery<CategoryBreakdownDto>;
}
