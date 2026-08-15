using Api.Helpers;
using Application.Workouts.Analytics.Dtos;
using Application.Workouts.Analytics.Queries.GetExerciseHistory;
using Application.Workouts.Analytics.Queries.GetPersonalRecords;
using Application.Workouts.Analytics.Queries.GetWorkoutSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// Training analytics. Everything here counts <b>completed</b> sessions only, and excludes warm-up sets.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/workouts/analytics")]
    public class WorkoutAnalyticsController(ISender sender) : ControllerBase
    {
        /// <summary>Headline numbers for a date range, plus volume split by muscle group.</summary>
        [HttpGet("summary")]
        public async Task<ActionResult<WorkoutSummaryDto>> GetSummaryAsync(
            [FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken cancellationToken = default)
        {
            var query = new GetWorkoutSummaryQuery(User.GetCurrentUserProfileId(), from, to);
            return Ok(await sender.Send(query, cancellationToken).ConfigureAwait(false));
        }

        /// <summary>Progress on one exercise — one point per session, with the estimated one-rep max.</summary>
        [HttpGet("exercise-history")]
        public async Task<ActionResult<ExerciseHistoryDto>> GetExerciseHistoryAsync(
            [FromQuery] int exerciseId,
            [FromQuery] DateOnly from,
            [FromQuery] DateOnly to,
            CancellationToken cancellationToken = default)
        {
            var query = new GetExerciseHistoryQuery(User.GetCurrentUserProfileId(), exerciseId, from, to);
            return Ok(await sender.Send(query, cancellationToken).ConfigureAwait(false));
        }

        /// <summary>All-time bests per exercise. No date range — records span the whole history.</summary>
        [HttpGet("personal-records")]
        public async Task<ActionResult<IEnumerable<PersonalRecordDto>>> GetPersonalRecordsAsync(
            CancellationToken cancellationToken = default)
        {
            var query = new GetPersonalRecordsQuery(User.GetCurrentUserProfileId());
            return Ok(await sender.Send(query, cancellationToken).ConfigureAwait(false));
        }
    }
}
