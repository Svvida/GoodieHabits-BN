using Api.Helpers;
using Application.Common.Dtos;
using Application.Workouts.Sessions.Commands.AbandonSession;
using Application.Workouts.Sessions.Commands.AddSessionExercise;
using Application.Workouts.Sessions.Commands.AddSet;
using Application.Workouts.Sessions.Commands.DeleteSession;
using Application.Workouts.Sessions.Commands.DeleteSet;
using Application.Workouts.Sessions.Commands.FinishSession;
using Application.Workouts.Sessions.Commands.LogSession;
using Application.Workouts.Sessions.Commands.RemoveSessionExercise;
using Application.Workouts.Sessions.Commands.StartSession;
using Application.Workouts.Sessions.Commands.UpdateSession;
using Application.Workouts.Sessions.Commands.UpdateSet;
using Application.Workouts.Sessions.Dtos;
using Application.Workouts.Sessions.Queries.GetActiveSession;
using Application.Workouts.Sessions.Queries.GetSessionById;
using Application.Workouts.Sessions.Queries.GetSessions;
using Domain.Enums;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// Performed training sessions. Every mutating endpoint returns the full session, so the client can
    /// repaint from one response.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/workouts/sessions")]
    public class WorkoutSessionsController(ISender sender, IMapper mapper) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<PagedResult<WorkoutSessionSummaryDto>>> GetAsync(
            [FromQuery] DateOnly? from = null,
            [FromQuery] DateOnly? to = null,
            [FromQuery] WorkoutSessionStatusEnum? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var query = new GetSessionsQuery(User.GetCurrentUserProfileId(), from, to, status, page, pageSize);
            return Ok(await sender.Send(query, cancellationToken).ConfigureAwait(false));
        }

        /// <summary>
        /// The session currently being logged, or <b>204 No Content</b> when nothing is running.
        /// <para>
        /// "Nothing in progress" is an ordinary state, not an error, so this is deliberately not a 404. The
        /// 204 is written explicitly rather than left to <c>Ok(null)</c>: ASP.NET Core's
        /// <c>HttpNoContentOutputFormatter</c> rewrites a null body to 204 anyway, and a controller that
        /// *looks* like it returns `200 null` while actually returning 204 is how a client ends up calling
        /// <c>response.json()</c> on an empty body.
        /// </para>
        /// </summary>
        [HttpGet("active")]
        [ProducesResponseType(typeof(WorkoutSessionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<WorkoutSessionDto>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            var query = new GetActiveSessionQuery(User.GetCurrentUserProfileId());
            var session = await sender.Send(query, cancellationToken).ConfigureAwait(false);

            return session is null ? NoContent() : Ok(session);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<WorkoutSessionDto>> GetByIdAsync(
            int id, CancellationToken cancellationToken = default)
        {
            var query = new GetSessionByIdQuery(id, User.GetCurrentUserProfileId());
            return Ok(await sender.Send(query, cancellationToken).ConfigureAwait(false));
        }

        /// <summary>
        /// Starts a session, from a routine or ad hoc. Returns 409 if one is already in progress — the message
        /// carries its id, so the client can offer "resume".
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<WorkoutSessionDto>> StartAsync(
            [FromBody] StartSessionRequest request, CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<StartSessionCommand>(request) with
            {
                UserProfileId = User.GetCurrentUserProfileId()
            };
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        /// <summary>Session metadata only (name, date, note). The log is edited through the endpoints below.</summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<WorkoutSessionDto>> UpdateAsync(
            int id, [FromBody] UpdateSessionRequest request, CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<UpdateSessionCommand>(request) with
            {
                SessionId = id,
                UserProfileId = User.GetCurrentUserProfileId()
            };
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        /// <summary>
        /// Bulk logging: replaces the whole exercise+set tree. Full replacement, so retrying the same payload
        /// after a flaky connection is harmless.
        /// </summary>
        [HttpPut("{id:int}/log")]
        public async Task<ActionResult<WorkoutSessionDto>> LogAsync(
            int id, [FromBody] LogSessionRequest request, CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<LogSessionCommand>(request) with
            {
                SessionId = id,
                UserProfileId = User.GetCurrentUserProfileId()
            };
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        [HttpPost("{id:int}/finish")]
        public async Task<ActionResult<WorkoutSessionDto>> FinishAsync(
            int id, CancellationToken cancellationToken = default)
        {
            var command = new FinishSessionCommand(id, User.GetCurrentUserProfileId());
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        [HttpPost("{id:int}/abandon")]
        public async Task<ActionResult<WorkoutSessionDto>> AbandonAsync(
            int id, CancellationToken cancellationToken = default)
        {
            var command = new AbandonSessionCommand(id, User.GetCurrentUserProfileId());
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        /// <summary>Adds an exercise mid-workout, optionally with its sets already filled in.</summary>
        [HttpPost("{id:int}/exercises")]
        public async Task<ActionResult<WorkoutSessionDto>> AddExerciseAsync(
            int id, [FromBody] SessionExerciseInput request, CancellationToken cancellationToken = default)
        {
            var command = new AddSessionExerciseCommand(id, request, User.GetCurrentUserProfileId());
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        [HttpDelete("{id:int}/exercises/{entryId:int}")]
        public async Task<ActionResult<WorkoutSessionDto>> RemoveExerciseAsync(
            int id, int entryId, CancellationToken cancellationToken = default)
        {
            var command = new RemoveSessionExerciseCommand(id, entryId, User.GetCurrentUserProfileId());
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        /// <summary>Live logging of a single set. The server assigns the set number.</summary>
        [HttpPost("{id:int}/exercises/{entryId:int}/sets")]
        public async Task<ActionResult<WorkoutSessionDto>> AddSetAsync(
            int id, int entryId, [FromBody] SessionSetInput request, CancellationToken cancellationToken = default)
        {
            var command = new AddSetCommand(id, entryId, request, User.GetCurrentUserProfileId());
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        [HttpPut("{id:int}/exercises/{entryId:int}/sets/{setId:int}")]
        public async Task<ActionResult<WorkoutSessionDto>> UpdateSetAsync(
            int id, int entryId, int setId, [FromBody] SessionSetInput request,
            CancellationToken cancellationToken = default)
        {
            var command = new UpdateSetCommand(id, entryId, setId, request, User.GetCurrentUserProfileId());
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        [HttpDelete("{id:int}/exercises/{entryId:int}/sets/{setId:int}")]
        public async Task<ActionResult<WorkoutSessionDto>> DeleteSetAsync(
            int id, int entryId, int setId, CancellationToken cancellationToken = default)
        {
            var command = new DeleteSetCommand(id, entryId, setId, User.GetCurrentUserProfileId());
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            await sender.Send(new DeleteSessionCommand(id, User.GetCurrentUserProfileId()), cancellationToken)
                .ConfigureAwait(false);

            return NoContent();
        }
    }
}
