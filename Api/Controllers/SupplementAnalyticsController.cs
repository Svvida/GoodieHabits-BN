using Api.Helpers;
using Application.Supplements.Analytics.Dtos;
using Application.Supplements.Analytics.Queries.GetAdherence;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/supplements/analytics")]
    public class SupplementAnalyticsController(ISender sender) : ControllerBase
    {
        /// <summary>
        /// Planned doses versus doses taken. A day counts toward the denominator only once it has fully
        /// elapsed in the user's local calendar, or once something was taken that day — so today never drags
        /// the number down while it is still running. A null rate means "nothing evaluated yet", not 0%.
        /// </summary>
        [HttpGet("adherence")]
        public async Task<ActionResult<SupplementAdherenceReportDto>> GetAdherenceAsync(
            [FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken cancellationToken = default)
        {
            var query = new GetAdherenceQuery(User.GetCurrentUserProfileId(), from, to);
            return Ok(await sender.Send(query, cancellationToken).ConfigureAwait(false));
        }
    }
}
