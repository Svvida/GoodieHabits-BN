using Api.Helpers;
using Application.Workouts.Settings.Commands.UpdateWeightUnit;
using Application.Workouts.Settings.Dtos;
using Application.Workouts.Settings.Queries.GetWorkoutSettings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>Mirrors <c>api/finance/settings</c>, including its "never convert history" rule.</summary>
    [ApiController]
    [Authorize]
    [Route("api/workouts/settings")]
    public class WorkoutSettingsController(ISender sender) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<WorkoutSettingsDto>> GetAsync(CancellationToken cancellationToken = default)
        {
            var query = new GetWorkoutSettingsQuery(User.GetCurrentUserProfileId());
            return Ok(await sender.Send(query, cancellationToken).ConfigureAwait(false));
        }

        /// <summary>Reinterprets stored weights in the new unit; it does not convert them.</summary>
        [HttpPut("weight-unit")]
        public async Task<ActionResult<WorkoutSettingsDto>> UpdateWeightUnitAsync(
            [FromBody] UpdateWeightUnitRequest request, CancellationToken cancellationToken = default)
        {
            var command = new UpdateWeightUnitCommand(request.WeightUnit, User.GetCurrentUserProfileId());
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }
    }
}
