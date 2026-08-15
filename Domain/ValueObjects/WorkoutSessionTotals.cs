namespace Domain.ValueObjects
{
    /// <summary>
    /// Rolled-up numbers for one training session. Computed on read, never stored — the fold is over a session's
    /// own sets, so there is nothing here worth denormalizing and drifting.
    /// </summary>
    /// <param name="ExerciseCount">Exercise entries in the session, including ones with no sets logged.</param>
    /// <param name="SetCount">Working sets. Warm-ups are excluded.</param>
    /// <param name="TotalReps">Sum of reps across working sets.</param>
    /// <param name="TotalVolume">Sum of <c>reps × weight</c> across working sets, in the user's weight unit.</param>
    /// <param name="DurationSeconds">Wall-clock length, or null while the session is still running.</param>
    public record WorkoutSessionTotals(
        int ExerciseCount,
        int SetCount,
        int TotalReps,
        decimal TotalVolume,
        int? DurationSeconds);
}
