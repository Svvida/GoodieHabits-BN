using Application.UserProfiles.Queries.GetUserProfiles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/user-profiles")]
    [Authorize]
    public class UserProfilesController(ISender sender) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<UserProfileSummaryDto>>> GetUserProfiles(
            CancellationToken cancellationToken,
            [FromQuery] string? nickname,
            [FromQuery] int limit = 10)
        {
            var query = new GetUserProfilesQuery(nickname, limit);
            var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
            return Ok(result);
        }
    }
}
