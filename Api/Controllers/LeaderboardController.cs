using Application.Dtos.Leaderboard;
using Application.Leaderboard.GetTopXp;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/leaderboard")]
    [Authorize]
    public class LeaderboardController(ISender sender) : ControllerBase
    {
        private readonly ISender _sender = sender;

        [HttpGet("xp")]
        public async Task<ActionResult<List<LeaderboardItemDto>>> GetXpLeaderboardAsync(
            CancellationToken cancellationToken = default)
        {
            var query = new GetTopXpQuery();
            var leaderboardItems = await _sender.Send(query, cancellationToken);

            return Ok(leaderboardItems);
        }
    }
}
