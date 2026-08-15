namespace Domain.Enums
{
    /// <summary>
    /// Declares which columns on a logged <c>WorkoutSet</c> are meaningful for an exercise. A single
    /// <c>Exercise</c> entity discriminated by this enum replaces a per-type class hierarchy — the same shape
    /// <c>Quest</c> and <c>FinanceTransaction</c> use — and it is what lets the client pick its input control
    /// without the backend knowing anything about the UI.
    /// <para>
    /// ⚠️ This is a value type: an <em>omitted</em> <c>metricType</c> deserializes to <see cref="Reps"/> (0),
    /// not null. <see cref="Reps"/> deliberately sits at 0 because it is the metric that demands the least
    /// data, so a client bug degrades to "we lost the weight column" rather than to a set that cannot be saved.
    /// </para>
    /// </summary>
    public enum ExerciseMetricEnum
    {
        /// <summary>Reps only — pull-ups, push-ups.</summary>
        Reps,

        /// <summary>Reps and weight — the default for resistance training.</summary>
        RepsAndWeight,

        /// <summary>Duration only — plank, dead hang.</summary>
        Time,

        /// <summary>Distance only — farmer's walk.</summary>
        Distance,

        /// <summary>Distance and duration — running, cycling, rowing.</summary>
        DistanceAndTime,
    }
}
