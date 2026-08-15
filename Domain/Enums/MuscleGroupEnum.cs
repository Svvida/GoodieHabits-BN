namespace Domain.Enums
{
    /// <summary>
    /// Primary muscle group an exercise trains. Used for filtering the library and for the per-group volume
    /// breakdown in analytics.
    /// <para>
    /// <see cref="Other"/> sits at 0 on purpose: the enum is a value type, so an omitted field deserializes to
    /// whatever is first, and "Other" is the only value that is never a confidently wrong answer.
    /// </para>
    /// </summary>
    public enum MuscleGroupEnum
    {
        Other,
        Chest,
        Back,
        Shoulders,
        Biceps,
        Triceps,
        Forearms,
        Abs,
        Glutes,
        Quadriceps,
        Hamstrings,
        Calves,
        FullBody,
        Cardio,
    }
}
