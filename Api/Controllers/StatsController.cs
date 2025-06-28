using Application.Dtos.Stats;
using Application.Dtos.UserProfileStats;
using Application.Interfaces;
using Domain;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/stats")]
    [Authorize]
    public class StatsController : ControllerBase
    {
        private readonly IStatsService _statsService;

        public StatsController(IStatsService statsService)
        {
            _statsService = statsService;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<GetUserProfileStatsDto>> GetUserProfileStats(CancellationToken cancellationToken = default)
        {
            string? accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid access token: missing account identifier.");

            var profileStats = await _statsService.GetUserProfileStatsAsync(accountId, cancellationToken).ConfigureAwait(false);

            return Ok(profileStats);
        }

        [HttpGet("extended")]
        public async Task<ActionResult<GetUserExtendedStatsDto>> GetUserExtendedStats(CancellationToken cancellationToken = default)
        {
            string? accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid access token: missing account identifier.");

            var extendedStats = await _statsService.GetUserExtendedStatsAsync(accountId, cancellationToken).ConfigureAwait(false);

            return Ok(extendedStats);
        }
    }
}
