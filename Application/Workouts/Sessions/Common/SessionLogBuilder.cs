using Application.Workouts.Sessions.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;

namespace Application.Workouts.Sessions.Common
{
    /// <summary>
    /// Turns the client's exercise+set payload into domain entities, resolving every referenced library
    /// exercise in one query.
    /// </summary>
    internal static class SessionLogBuilder
    {
        /// <summary>
        /// All-or-nothing: if any exercise id is unknown or belongs to someone else, the whole request is
        /// rejected and the session is left exactly as it was.
        /// <para>
        /// Sets are added through <c>WorkoutSessionExercise.AddSet</c> rather than constructed directly, so each
        /// one is validated against the metric snapshotted onto its entry — that check is the reason sets are
        /// not their own aggregate.
        /// </para>
        /// </summary>
        public static async Task<List<WorkoutSessionExercise>> BuildAsync(
            IUnitOfWork unitOfWork,
            IReadOnlyList<SessionExerciseInput> inputs,
            int userProfileId,
            DateTime defaultCompletedAtUtc,
            CancellationToken cancellationToken)
        {
            if (inputs.Count == 0)
                return [];

            var ids = inputs.Select(i => i.ExerciseId).Distinct().ToList();

            var found = await unitOfWork.Exercises
                .GetVisibleByIdsAsync(ids, userProfileId, cancellationToken)
                .ConfigureAwait(false);

            var byId = found.ToDictionary(e => e.Id);
            var missing = ids.Where(id => !byId.ContainsKey(id)).ToList();

            if (missing.Count > 0)
                throw new NotFoundException(
                    $"Exercises not found or not available to you: {string.Join(", ", missing)}.");

            var entries = new List<WorkoutSessionExercise>(inputs.Count);

            for (var index = 0; index < inputs.Count; index++)
            {
                var input = inputs[index];

                var entry = WorkoutSessionExercise.CreateFrom(
                    byId[input.ExerciseId],
                    index,
                    input.TargetSets,
                    input.TargetReps,
                    input.TargetWeight,
                    input.TargetDurationSeconds,
                    input.TargetDistance,
                    input.RestSeconds,
                    input.Note);

                foreach (var set in input.Sets ?? [])
                {
                    entry.AddSet(
                        set.CompletedAt ?? defaultCompletedAtUtc,
                        set.Reps,
                        set.Weight,
                        set.DurationSeconds,
                        set.Distance,
                        set.Rpe,
                        set.SetType);
                }

                entries.Add(entry);
            }

            return entries;
        }
    }
}
