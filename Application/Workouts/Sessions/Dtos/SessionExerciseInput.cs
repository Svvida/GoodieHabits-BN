namespace Application.Workouts.Sessions.Dtos
{
    /// <summary>
    /// One exercise, with its sets, as the bulk logging endpoint receives it. Position in the array is the
    /// order — there is no <c>order</c> field, for the same reason routines don't have one.
    /// </summary>
    public record SessionExerciseInput(
        int ExerciseId,
        int? TargetSets = null,
        int? TargetReps = null,
        decimal? TargetWeight = null,
        int? TargetDurationSeconds = null,
        decimal? TargetDistance = null,
        int? RestSeconds = null,
        string? Note = null,
        IReadOnlyList<SessionSetInput>? Sets = null);
}
