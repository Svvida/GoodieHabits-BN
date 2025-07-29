using Api.Helpers;
using Application.Dtos.Stats;
using Application.Dtos.UserProfileStats;
using Application.Statistics.Queries.GetProfileStats;
using Application.Statistics.Queries.GetUserExtendedStats;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/stats")]
    [Authorize]
    public class StatsController(ISender sender) : ControllerBase
    {
        private readonly ISender _sender = sender;

        [HttpGet("profile")]
        public async Task<ActionResult<GetUserProfileStatsDto>> GetUserProfileStats(CancellationToken cancellationToken = default)
        {
            var accountId = JwtHelpers.GetCurrentUserId(User);

            var query = new GetUserProfileStatsQuery(accountId);

            var profileStats = await _sender.Send(query, cancellationToken);

            return Ok(profileStats);
        }

        [HttpGet("extended")]
        public async Task<ActionResult<GetUserExtendedStatsDto>> GetUserExtendedStats(CancellationToken cancellationToken = default)
        {
            var accountId = JwtHelpers.GetCurrentUserId(User);

            var query = new GetUserExtendedStatsQuery(accountId);

            var extendedStats = await _sender.Send(query, cancellationToken);

            return Ok(extendedStats);
        }
    }
}
