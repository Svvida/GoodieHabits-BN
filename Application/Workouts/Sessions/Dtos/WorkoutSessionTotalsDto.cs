using Domain.ValueObjects;

namespace Application.Workouts.Sessions.Dtos
{
    /// <summary>
    /// Rolled up from the session's own sets on every read — nothing here is stored, so nothing here can drift.
    /// Warm-up sets are excluded throughout: they are preparation, not work.
    /// </summary>
    public class WorkoutSessionTotalsDto
    {
        public int ExerciseCount { get; set; }
        public int SetCount { get; set; }
        public int TotalReps { get; set; }

        /// <summary>Sum of <c>reps × weight</c>, in the user's weight unit (<c>GET /api/workouts/settings</c>).</summary>
        public decimal TotalVolume { get; set; }

        public static WorkoutSessionTotalsDto From(WorkoutSessionTotals totals) => new()
        {
            ExerciseCount = totals.ExerciseCount,
            SetCount = totals.SetCount,
            TotalReps = totals.TotalReps,
            TotalVolume = totals.TotalVolume,
        };
    }
}
