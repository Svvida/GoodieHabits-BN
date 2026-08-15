namespace Domain.Models
{
    /// <summary>
    /// Projection type for the grouped personal-records query — the same role <see cref="MonthlyTotal"/> plays
    /// for the finance opening balance. Never an entity, never tracked.
    /// <para>
    /// A fold over all history is a grouped <c>SELECT</c> of a few dozen rows at this scale, so records are
    /// computed on read rather than denormalized onto a column that would have to be kept in step with every
    /// edited set.
    /// </para>
    /// <para>
    /// ⚠️ Only plain aggregates live here. The estimated one-rep max is deliberately absent: Epley's
    /// single-rep special case and its high-rep cutoff do not survive translation to SQL reliably, and a
    /// personal record that reads 3% high is worse than one that isn't shown. The per-set estimate is on every
    /// set in session details and in the exercise-history endpoint, both of which fold in memory.
    /// </para>
    /// </summary>
    /// <param name="ExerciseId">The library exercise. Deleted exercises drop out of records entirely.</param>
    /// <param name="ExerciseName">Snapshot taken from the most recent session entry.</param>
    /// <param name="MaxWeight">Heaviest working set, in the user's weight unit.</param>
    /// <param name="MaxReps">Most reps in a single working set.</param>
    /// <param name="MaxSetVolume">Best <c>reps × weight</c> in a single working set.</param>
    /// <param name="SetCount">Working sets ever logged. Warm-ups are excluded throughout.</param>
    /// <param name="LastPerformedOn">Calendar date of the most recent session containing it.</param>
    public record ExerciseBest(
        int ExerciseId,
        string ExerciseName,
        decimal? MaxWeight,
        int? MaxReps,
        decimal? MaxSetVolume,
        int SetCount,
        DateOnly LastPerformedOn);
}
