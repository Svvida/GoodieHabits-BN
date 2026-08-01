namespace Domain.Enums
{
    /// <summary>How a single quest occurrence period turned out, as of the user's local today.</summary>
    public enum QuestPeriodOutcomeEnum
    {
        /// <summary>The user completed the quest in this period.</summary>
        Completed,

        /// <summary>The period elapsed without a completion.</summary>
        Missed,

        /// <summary>The period is in progress or still in the future — not yet judged either way.</summary>
        Pending
    }
}
