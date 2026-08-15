using Api.Helpers;
using Application.Workouts.Exercises.Commands.CreateExercise;
using Application.Workouts.Exercises.Commands.DeleteExercise;
using Application.Workouts.Exercises.Commands.SetExerciseArchived;
using Application.Workouts.Exercises.Commands.UpdateExercise;
using Application.Workouts.Exercises.Dtos;
using Application.Workouts.Exercises.Queries.GetExercises;
using Domain.Enums;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/workouts/exercises")]
    public class ExercisesController(ISender sender, IMapper mapper) : ControllerBase
    {
        /// <summary>Seeded system exercises plus the caller's own. Every filter is optional.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExerciseDto>>> GetAsync(
            [FromQuery] MuscleGroupEnum? muscleGroup = null,
            [FromQuery] ExerciseMetricEnum? metricType = null,
            [FromQuery] string? search = null,
            [FromQuery] bool includeArchived = false,
            CancellationToken cancellationToken = default)
        {
            var query = new GetExercisesQuery(
                User.GetCurrentUserProfileId(), muscleGroup, metricType, search, includeArchived);

            return Ok(await sender.Send(query, cancellationToken).ConfigureAwait(false));
        }

        [HttpPost]
        public async Task<ActionResult<ExerciseDto>> CreateAsync(
            [FromBody] CreateExerciseRequest request, CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<CreateExerciseCommand>(request) with
            {
                UserProfileId = User.GetCurrentUserProfileId()
            };
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ExerciseDto>> UpdateAsync(
            int id, [FromBody] UpdateExerciseRequest request, CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<UpdateExerciseCommand>(request) with
            {
                ExerciseId = id,
                UserProfileId = User.GetCurrentUserProfileId()
            };
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        /// <summary>Hides an exercise from the picker while leaving it in routines and past sessions.</summary>
        [HttpPatch("{id:int}/archived")]
        public async Task<ActionResult<ExerciseDto>> SetArchivedAsync(
            int id, [FromBody] SetExerciseArchivedRequest request, CancellationToken cancellationToken = default)
        {
            var command = new SetExerciseArchivedCommand(id, request.IsArchived, User.GetCurrentUserProfileId());
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        /// <summary>
        /// Blocked (409) while a routine still plans this exercise. Past sessions never block it — they keep a
        /// snapshot of the name and metric.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            await sender.Send(new DeleteExerciseCommand(id, User.GetCurrentUserProfileId()), cancellationToken)
                .ConfigureAwait(false);

            return NoContent();
        }
    }
}
