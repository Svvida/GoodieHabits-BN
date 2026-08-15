namespace Application.Workouts.Routines.Dtos
{
    /// <summary>
    /// One planned exercise as the client sends it.
    /// <para>
    /// There is deliberately no <c>Order</c> field: <b>the position in the array is the order</b>. A separate
    /// order value the server then has to reconcile with array order is a contradiction waiting to happen, and
    /// both create and update replace the list wholesale anyway.
    /// </para>
    /// <para>Every target is optional — a routine that only fixes the sequence is a legitimate plan.</para>
    /// </summary>
    public record RoutineExerciseInput(
        int ExerciseId,
        int? TargetSets = null,
        int? TargetReps = null,
        decimal? TargetWeight = null,
        int? TargetDurationSeconds = null,
        decimal? TargetDistance = null,
        int? RestSeconds = null,
        string? Note = null);
}
