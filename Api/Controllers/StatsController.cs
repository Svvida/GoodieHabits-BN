using Api.Helpers;
using Application.Statistics.Queries.GetUserExtendedStats;
using Application.Statistics.Queries.GetUserProfileStats;
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

        [HttpGet("profile")]
        public async Task<ActionResult<GetUserProfileStatsResponse>> GetUserProfileStats(CancellationToken cancellationToken = default)
        {
            var query = new GetUserProfileStatsQuery(JwtHelpers.GetCurrentUserId(User));
            var profileStats = await sender.Send(query, cancellationToken);
            return Ok(profileStats);
        }

        [HttpGet("extended")]
        public async Task<ActionResult<GetUserExtendedStatsResponse>> GetUserExtendedStats(CancellationToken cancellationToken = default)
        {
            var query = new GetUserExtendedStatsQuery(JwtHelpers.GetCurrentUserId(User));
            var extendedStats = await sender.Send(query, cancellationToken);
            return Ok(extendedStats);
        }
    }
}
