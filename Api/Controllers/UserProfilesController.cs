using Api.Helpers;
using Application.UserProfiles.Queries.GetUserProfileForPublicDisplay;
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
            [FromQuery] string? nickname,
            [FromQuery] int limit = 10,
            CancellationToken cancellationToken = default)
        {
            var query = new GetUserProfilesQuery(nickname, limit);
            var result = await sender.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{userProfileId}")]
        public async Task<ActionResult<BaseUserProfileDto>> GetUserProfileById(
            int userProfileId,
            CancellationToken cancellationToken = default)
        {
            var query = new GetUserProfileForPublicDisplayQuery(JwtHelpers.GetCurrentUserProfileId(User), userProfileId);
            var result = await sender.Send(query, cancellationToken);
            return Ok(result);
        }
    }
}
