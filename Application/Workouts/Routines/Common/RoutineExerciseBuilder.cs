using Application.Workouts.Routines.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;

namespace Application.Workouts.Routines.Common
{
    /// <summary>
    /// Turns the client's exercise list into domain items, shared by the create and update handlers because
    /// both replace the whole list.
    /// </summary>
    internal static class RoutineExerciseBuilder
    {
        /// <summary>
        /// Resolves every referenced exercise in one query and rejects the whole request if any of them is
        /// missing or belongs to someone else — all-or-nothing, so a typo can never produce a half-built
        /// routine. Archived exercises are accepted: a routine may legitimately still contain one.
        /// </summary>
        public static async Task<List<WorkoutRoutineExercise>> BuildAsync(
            IUnitOfWork unitOfWork,
            IReadOnlyList<RoutineExerciseInput> inputs,
            int userProfileId,
            CancellationToken cancellationToken)
        {
            if (inputs.Count == 0)
                return [];

            var ids = inputs.Select(i => i.ExerciseId).Distinct().ToList();

            var found = await unitOfWork.Exercises
                .GetVisibleByIdsAsync(ids, userProfileId, cancellationToken)
                .ConfigureAwait(false);

            var foundIds = found.Select(e => e.Id).ToHashSet();
            var missing = ids.Where(id => !foundIds.Contains(id)).ToList();

            if (missing.Count > 0)
                throw new NotFoundException(
                    $"Exercises not found or not available to you: {string.Join(", ", missing)}.");

            // Position in the array is the order.
            return [.. inputs.Select((input, index) => WorkoutRoutineExercise.Create(
                input.ExerciseId,
                index,
                input.TargetSets,
                input.TargetReps,
                input.TargetWeight,
                input.TargetDurationSeconds,
                input.TargetDistance,
                input.RestSeconds,
                input.Note))];
        }
    }
}
