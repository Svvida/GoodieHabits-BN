using Application.Dtos.Leaderboard;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/leaderboard")]
    [Authorize]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;
        public LeaderboardController(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        [HttpGet("xp")]
        public async Task<ActionResult<List<LeaderboardItemDto>>> GetXpLeaderboardAsync(
            CancellationToken cancellationToken = default)
        {
            var leaderboardItems = await _leaderboardService.GetTopXpLeaderboardAsync(cancellationToken);

            return Ok(leaderboardItems);
        }
    }
}
