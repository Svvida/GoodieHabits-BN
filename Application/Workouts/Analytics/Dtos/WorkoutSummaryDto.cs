using Domain.Enums;

namespace Application.Workouts.Analytics.Dtos
{
    /// <summary>Training volume for one muscle group over the requested period.</summary>
    public class MuscleGroupVolumeDto
    {
        public MuscleGroupEnum MuscleGroup { get; set; }
        public int SetCount { get; set; }
        public int TotalReps { get; set; }
        public decimal TotalVolume { get; set; }
    }

    /// <summary>
    /// Headline numbers for a date range. Aggregated in memory over the sessions in that range, the way every
    /// finance analytics handler works — the math lives in <c>WorkoutVolumeCalculator</c>, the repository only
    /// fetches rows.
    /// <para>
    /// Only <b>completed</b> sessions count. An abandoned session is not training you did, and an in-progress
    /// one isn't finished; letting either into the totals would make today's numbers move under the user.
    /// </para>
    /// </summary>
    public class WorkoutSummaryDto
    {
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }

        public int SessionCount { get; set; }
        public int ExerciseCount { get; set; }
        public int SetCount { get; set; }
        public int TotalReps { get; set; }

        /// <summary>Sum of <c>reps × weight</c>, in the user's weight unit. Warm-ups excluded.</summary>
        public decimal TotalVolume { get; set; }

        /// <summary>Total logged training time, or 0 when no session in the range recorded one.</summary>
        public int TotalDurationSeconds { get; set; }

        /// <summary>Average session length in seconds, or null when nothing was completed.</summary>
        public int? AverageDurationSeconds { get; set; }

        /// <summary>
        /// Volume split by the muscle group each exercise was classified under <em>at read time</em>. Entries
        /// whose library exercise was deleted fall under <c>Other</c>.
        /// </summary>
        public List<MuscleGroupVolumeDto> ByMuscleGroup { get; set; } = [];
    }
}
