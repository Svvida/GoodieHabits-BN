using Api.Helpers;
using Application.Supplements.Catalog.Commands.AddSupplementSlot;
using Application.Supplements.Catalog.Commands.CreateSupplement;
using Application.Supplements.Catalog.Commands.DeleteSupplement;
using Application.Supplements.Catalog.Commands.DeleteSupplementSlot;
using Application.Supplements.Catalog.Commands.SetSupplementActive;
using Application.Supplements.Catalog.Commands.UpdateSupplement;
using Application.Supplements.Catalog.Commands.UpdateSupplementSlot;
using Application.Supplements.Catalog.Dtos;
using Application.Supplements.Catalog.Queries.GetSupplements;
using Application.Supplements.Intakes.Commands.DeleteIntake;
using Application.Supplements.Intakes.Commands.LogAdHocIntake;
using Application.Supplements.Intakes.Commands.ToggleIntake;
using Application.Supplements.Intakes.Dtos;
using Application.Supplements.Intakes.Queries.GetChecklist;
using Application.Supplements.Intakes.Queries.GetIntakes;
using Domain.Enums;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// The supplement catalog, its schedule, and the daily checklist.
    /// <para>
    /// This module is independent of workouts on purpose — a supplement plan has to work on a rest day. The
    /// in-training panel is the same <c>/checklist</c> filtered to <c>PreWorkout</c>/<c>PostWorkout</c>, with
    /// <c>workoutSessionId</c> passed when ticking. That is the whole integration.
    /// </para>
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/supplements")]
    public class SupplementsController(ISender sender, IMapper mapper) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SupplementDto>>> GetAsync(
            [FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            var query = new GetSupplementsQuery(User.GetCurrentUserProfileId(), includeInactive);
            return Ok(await sender.Send(query, cancellationToken).ConfigureAwait(false));
        }

        [HttpPost]
        public async Task<ActionResult<SupplementDto>> CreateAsync(
            [FromBody] CreateSupplementRequest request, CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<CreateSupplementCommand>(request) with
            {
                UserProfileId = User.GetCurrentUserProfileId()
            };
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<SupplementDto>> UpdateAsync(
            int id, [FromBody] UpdateSupplementRequest request, CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<UpdateSupplementCommand>(request) with
            {
                SupplementId = id,
                UserProfileId = User.GetCurrentUserProfileId()
            };
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        /// <summary>The retire path — drops it off the checklist while keeping every dose ever logged.</summary>
        [HttpPatch("{id:int}/active")]
        public async Task<ActionResult<SupplementDto>> SetActiveAsync(
            int id, [FromBody] SetSupplementActiveRequest request, CancellationToken cancellationToken = default)
        {
            var command = new SetSupplementActiveCommand(id, request.IsActive, User.GetCurrentUserProfileId());
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        /// <summary>Refused (409) once anything has been logged against it — deactivate instead.</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            await sender.Send(new DeleteSupplementCommand(id, User.GetCurrentUserProfileId()), cancellationToken)
                .ConfigureAwait(false);

            return NoContent();
        }

        [HttpPost("{id:int}/slots")]
        public async Task<ActionResult<SupplementDto>> AddSlotAsync(
            int id, [FromBody] SupplementSlotRequest request, CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<AddSupplementSlotCommand>(request) with
            {
                SupplementId = id,
                UserProfileId = User.GetCurrentUserProfileId()
            };
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        [HttpPut("{id:int}/slots/{slotId:int}")]
        public async Task<ActionResult<SupplementDto>> UpdateSlotAsync(
            int id, int slotId, [FromBody] SupplementSlotRequest request, CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<UpdateSupplementSlotCommand>(request) with
            {
                SupplementId = id,
                SlotId = slotId,
                UserProfileId = User.GetCurrentUserProfileId()
            };
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        /// <summary>Doses already taken against the slot are kept — they become ad-hoc records.</summary>
        [HttpDelete("{id:int}/slots/{slotId:int}")]
        public async Task<ActionResult<SupplementDto>> DeleteSlotAsync(
            int id, int slotId, CancellationToken cancellationToken = default)
        {
            var command = new DeleteSupplementSlotCommand(id, slotId, User.GetCurrentUserProfileId());
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        /// <summary>
        /// One day's plan and what has been ticked off. Pass <c>timing</c> (repeatable) to get just the
        /// in-training slice: <c>?timing=PreWorkout&amp;timing=PostWorkout</c>.
        /// </summary>
        [HttpGet("checklist")]
        public async Task<ActionResult<SupplementChecklistDto>> GetChecklistAsync(
            [FromQuery] DateOnly date,
            [FromQuery] SupplementTimingEnum[]? timing = null,
            CancellationToken cancellationToken = default)
        {
            var query = new GetChecklistQuery(User.GetCurrentUserProfileId(), date, timing);
            return Ok(await sender.Send(query, cancellationToken).ConfigureAwait(false));
        }

        /// <summary>Raw intake history over an inclusive range — planned and ad-hoc alike.</summary>
        [HttpGet("intakes")]
        public async Task<ActionResult<IEnumerable<SupplementIntakeDto>>> GetIntakesAsync(
            [FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken cancellationToken = default)
        {
            var query = new GetIntakesQuery(User.GetCurrentUserProfileId(), from, to);
            return Ok(await sender.Send(query, cancellationToken).ConfigureAwait(false));
        }

        /// <summary>
        /// The checkbox. Idempotent in both directions — send the intent, not a row id. Returns the whole day's
        /// checklist.
        /// </summary>
        [HttpPut("intakes")]
        public async Task<ActionResult<SupplementChecklistDto>> ToggleIntakeAsync(
            [FromBody] ToggleIntakeRequest request, CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<ToggleIntakeCommand>(request) with
            {
                UserProfileId = User.GetCurrentUserProfileId()
            };
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        /// <summary>
        /// A dose no slot planned. Deliberately repeatable, unlike the checkbox — undo with
        /// <c>DELETE /intakes/{id}</c>.
        /// </summary>
        [HttpPost("intakes")]
        public async Task<ActionResult<SupplementChecklistDto>> LogAdHocIntakeAsync(
            [FromBody] LogAdHocIntakeRequest request, CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<LogAdHocIntakeCommand>(request) with
            {
                UserProfileId = User.GetCurrentUserProfileId()
            };
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        [HttpDelete("intakes/{intakeId:int}")]
        public async Task<ActionResult<SupplementChecklistDto>> DeleteIntakeAsync(
            int intakeId, CancellationToken cancellationToken = default)
        {
            var command = new DeleteIntakeCommand(intakeId, User.GetCurrentUserProfileId());
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }
    }
}
