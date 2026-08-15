using Domain.Models;
using Domain.ValueObjects;

namespace Domain.Calculators
{
    /// <summary>
    /// Pure training-volume arithmetic. Side-effect free and DB-free, which is what keeps it honest: the
    /// InMemory provider used in tests would happily pass an aggregation that real SQL gets wrong, so the math
    /// lives here and the repositories only fetch rows.
    /// </summary>
    public static class WorkoutVolumeCalculator
    {
        /// <summary>
        /// Sum of <c>reps × weight</c>. Warm-ups are excluded unless asked for — they are preparation, not work,
        /// and counting them would inflate every trend the moment a user starts logging them properly.
        /// <para>
        /// A set with no weight contributes 0. Bodyweight work therefore has no volume until bodyweight
        /// tracking exists (a deliberate backlog item) — rep counts still carry it in
        /// <see cref="WorkoutSessionTotals.TotalReps"/>.
        /// </para>
        /// </summary>
        public static decimal CalculateVolume(IEnumerable<WorkoutSet> sets, bool includeWarmups = false)
        {
            ArgumentNullException.ThrowIfNull(sets);

            return sets
                .Where(s => includeWarmups || !s.IsWarmup)
                .Sum(s => (s.Reps ?? 0) * (s.Weight ?? 0m));
        }

        public static WorkoutSessionTotals CalculateSessionTotals(WorkoutSession session, bool includeWarmups = false)
        {
            ArgumentNullException.ThrowIfNull(session);

            var sets = session.Exercises
                .SelectMany(e => e.Sets)
                .Where(s => includeWarmups || !s.IsWarmup)
                .ToList();

            return new WorkoutSessionTotals(
                session.Exercises.Count,
                sets.Count,
                sets.Sum(s => s.Reps ?? 0),
                sets.Sum(s => (s.Reps ?? 0) * (s.Weight ?? 0m)),
                session.DurationSeconds);
        }
    }
}
