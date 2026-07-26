using Application.Common.Interfaces;
using Application.Finance.Analytics.Dtos;

namespace Application.Finance.Analytics.Queries.GetSpendingTrend
{
    // Income/expense/net per month for the `Months` months ending at (EndYear, EndMonth). Spans year boundaries.
    public record GetSpendingTrendQuery(int UserProfileId, int EndYear, int EndMonth, int Months)
        : IQuery<SpendingTrendDto>;
}
