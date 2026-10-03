namespace Domain.Enums
{
    /// <summary>
    /// The calendar unit a quest's accountability period is measured in. This replaces
    /// <see cref="QuestTypeEnum"/> as the thing that decides which periods exist.
    /// </summary>
    public enum PeriodUnitEnum
    {
        /// <summary>No recurrence — the quest has a single period spanning its whole active range.</summary>
        None,
        Day,
        Week,
        Month,
        Year
    }
}
