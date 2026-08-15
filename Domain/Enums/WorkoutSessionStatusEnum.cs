namespace Domain.Enums
{
    /// <summary>
    /// Lifecycle of a training session. <see cref="InProgress"/> at 0 is correct rather than merely safe — a
    /// session is created by starting it.
    /// </summary>
    public enum WorkoutSessionStatusEnum
    {
        /// <summary>Being logged right now. At most one per user (a second start is a conflict).</summary>
        InProgress,

        /// <summary>Finished. Raises <c>WorkoutSessionCompletedEvent</c>.</summary>
        Completed,

        /// <summary>Started and given up on. Kept rather than deleted, so the history stays honest.</summary>
        Abandoned,
    }
}
