namespace Domain.Enums
{
    /// <summary>
    /// When a scheduled dose is meant to be taken. Coarse buckets rather than clock times, because that is how
    /// people actually describe a supplement plan ("rano", "30 min przed treningiem"); an exact
    /// <c>TimeOfDay</c> and a workout-relative <c>OffsetMinutes</c> are both optional refinements on the slot.
    /// <para>
    /// <see cref="PreWorkout"/> / <see cref="PostWorkout"/> are what the in-training supplement panel filters
    /// on — that filter is the entire integration between the supplements and workouts modules.
    /// </para>
    /// </summary>
    public enum SupplementTimingEnum
    {
        Morning,
        Midday,
        Afternoon,
        Evening,
        Night,
        PreWorkout,
        PostWorkout,
        WithMeal,

        /// <summary>A specific clock time. Requires <c>TimeOfDay</c> on the slot.</summary>
        Custom,
    }
}
