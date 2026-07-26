using Api.Helpers;
using Application.Finance.Analytics.Dtos;
using Application.Finance.Analytics.Queries.GetBudgetProgress;
using Application.Finance.Analytics.Queries.GetCategoryBreakdown;
using Application.Finance.Analytics.Queries.GetMonthlySummary;
using Application.Finance.Analytics.Queries.GetSpendingTrend;
using Application.Finance.Analytics.Queries.GetYearlySummary;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/finance/analytics")]
    public class FinanceAnalyticsController(ISender sender) : ControllerBase
    {
        [HttpGet("monthly-summary")]
        public async Task<ActionResult<MonthlySummaryDto>> GetMonthlySummaryAsync(
            [FromQuery] int year, [FromQuery] int month, CancellationToken cancellationToken = default)
        {
            var query = new GetMonthlySummaryQuery(User.GetCurrentUserProfileId(), year, month);
            return Ok(await sender.Send(query, cancellationToken).ConfigureAwait(false));
        }

        [HttpGet("yearly-summary")]
        public async Task<ActionResult<YearlySummaryDto>> GetYearlySummaryAsync(
            [FromQuery] int year, CancellationToken cancellationToken = default)
        {
            var query = new GetYearlySummaryQuery(User.GetCurrentUserProfileId(), year);
            return Ok(await sender.Send(query, cancellationToken).ConfigureAwait(false));
        }

        [HttpGet("category-breakdown")]
        public async Task<ActionResult<CategoryBreakdownDto>> GetCategoryBreakdownAsync(
            [FromQuery] FinanceTransactionTypeEnum type,
            [FromQuery] BudgetPeriodEnum period,
            [FromQuery] int year,
            [FromQuery] int? month = null,
            CancellationToken cancellationToken = default)
        {
            var query = new GetCategoryBreakdownQuery(User.GetCurrentUserProfileId(), type, period, year, month);
            return Ok(await sender.Send(query, cancellationToken).ConfigureAwait(false));
        }

        [HttpGet("budget-progress")]
        public async Task<ActionResult<IEnumerable<BudgetProgressItemDto>>> GetBudgetProgressAsync(
            [FromQuery] int year, [FromQuery] int? month = null, CancellationToken cancellationToken = default)
        {
            var query = new GetBudgetProgressQuery(User.GetCurrentUserProfileId(), year, month);
            return Ok(await sender.Send(query, cancellationToken).ConfigureAwait(false));
        }

        [HttpGet("spending-trend")]
        public async Task<ActionResult<SpendingTrendDto>> GetSpendingTrendAsync(
            [FromQuery] int endYear,
            [FromQuery] int endMonth,
            [FromQuery] int months = 12,
            CancellationToken cancellationToken = default)
        {
            var query = new GetSpendingTrendQuery(User.GetCurrentUserProfileId(), endYear, endMonth, months);
            return Ok(await sender.Send(query, cancellationToken).ConfigureAwait(false));
        }
    }
}
