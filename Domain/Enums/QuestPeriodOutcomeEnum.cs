namespace Domain.Enums
{
    /// <summary>How a single quest occurrence period turned out, as of the user's local today.</summary>
    public enum QuestPeriodOutcomeEnum
    {
        /// <summary>The user reached the period's target.</summary>
        Completed,

        /// <summary>The period elapsed with no progress at all.</summary>
        Missed,

        /// <summary>The period is in progress or still in the future — not yet judged either way.</summary>
        Pending,

        /// <summary>
        /// The period elapsed with some progress, but short of its target ("1 of 2"). Counts as a miss
        /// everywhere a rate or a streak is computed; it exists so the UI can show partial credit
        /// instead of plain red.
        /// </summary>
        Partial,

        /// <summary>The user excused this period. Excluded from rates, and does not break a streak. Phase 2.</summary>
        Skipped
    }
}
