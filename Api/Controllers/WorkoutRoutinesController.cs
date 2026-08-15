using Api.Helpers;
using Application.Workouts.Routines.Commands.CreateRoutine;
using Application.Workouts.Routines.Commands.DeleteRoutine;
using Application.Workouts.Routines.Commands.SetRoutineArchived;
using Application.Workouts.Routines.Commands.UpdateRoutine;
using Application.Workouts.Routines.Dtos;
using Application.Workouts.Routines.Queries.GetRoutineById;
using Application.Workouts.Routines.Queries.GetRoutines;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/workouts/routines")]
    public class WorkoutRoutinesController(ISender sender, IMapper mapper) : ControllerBase
    {
        /// <summary>The saved templates, each with its exercises — this is the "pick a routine" list.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkoutRoutineDto>>> GetAsync(
            [FromQuery] bool includeArchived = false, CancellationToken cancellationToken = default)
        {
            var query = new GetRoutinesQuery(User.GetCurrentUserProfileId(), includeArchived);
            return Ok(await sender.Send(query, cancellationToken).ConfigureAwait(false));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<WorkoutRoutineDto>> GetByIdAsync(
            int id, CancellationToken cancellationToken = default)
        {
            var query = new GetRoutineByIdQuery(id, User.GetCurrentUserProfileId());
            return Ok(await sender.Send(query, cancellationToken).ConfigureAwait(false));
        }

        [HttpPost]
        public async Task<ActionResult<WorkoutRoutineDto>> CreateAsync(
            [FromBody] CreateRoutineRequest request, CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<CreateRoutineCommand>(request) with
            {
                UserProfileId = User.GetCurrentUserProfileId()
            };
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        /// <summary>Full replacement, including the exercise list. The array order is the workout order.</summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<WorkoutRoutineDto>> UpdateAsync(
            int id, [FromBody] UpdateRoutineRequest request, CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<UpdateRoutineCommand>(request) with
            {
                RoutineId = id,
                UserProfileId = User.GetCurrentUserProfileId()
            };
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        [HttpPatch("{id:int}/archived")]
        public async Task<ActionResult<WorkoutRoutineDto>> SetArchivedAsync(
            int id, [FromBody] SetRoutineArchivedRequest request, CancellationToken cancellationToken = default)
        {
            var command = new SetRoutineArchivedCommand(id, request.IsArchived, User.GetCurrentUserProfileId());
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        /// <summary>Sessions already performed from this routine are kept; they simply lose the template link.</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            await sender.Send(new DeleteRoutineCommand(id, User.GetCurrentUserProfileId()), cancellationToken)
                .ConfigureAwait(false);

            return NoContent();
        }
    }
}
