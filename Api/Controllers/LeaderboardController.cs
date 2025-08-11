using Application.Leaderboard.Queries.GetTopXp;
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
        [HttpGet("xp")]
        public async Task<ActionResult<GetTopXpResponse>> GetXpLeaderboardAsync(
            CancellationToken cancellationToken = default)
        {
            var query = new GetTopXpQuery();
            var leaderboardItems = await sender.Send(query, cancellationToken);

            return Ok(leaderboardItems);
        }
    }
}
