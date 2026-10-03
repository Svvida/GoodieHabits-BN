namespace Domain.ValueObjects
{
    /// <summary>
    /// One calendar period a quest is accountable for, in the user's local calendar. Both bounds are
    /// inclusive.
    /// <para>
    /// <see cref="Start"/>/<see cref="End"/> are the period as the user actually experiences it — clipped
    /// to the quest's active range. <see cref="FullStart"/>/<see cref="FullEnd"/> are the untruncated
    /// calendar unit. They differ only at the very start and end of a quest's life, and the difference is
    /// what a partial period's target is prorated by: a "3 times a week" quest created on a Saturday must
    /// not open with a guaranteed failure.
    /// </para>
    /// </summary>
    public readonly record struct QuestPeriodWindow(DateOnly Start, DateOnly End, DateOnly FullStart, DateOnly FullEnd)
    {
        public QuestPeriodWindow(DateOnly start, DateOnly end) : this(start, end, start, end) { }

        public int Days => End.DayNumber - Start.DayNumber + 1;

        public int FullDays => FullEnd.DayNumber - FullStart.DayNumber + 1;

        public bool IsPartial => Days < FullDays;

        public bool Covers(DateOnly date) => Start <= date && date <= End;
    }

    /// <summary>
    /// The fixed points period generation is computed against: where the recurrence counts from, and the
    /// range the quest is alive for.
    /// </summary>
    /// <param name="Anchor">
    /// What "every N units" counts from — the quest's start date, falling back to the local date it was
    /// created on. Changing it re-phases the whole series, so it is deliberately not the current date.
    /// </param>
    public readonly record struct QuestScheduleBounds(DateOnly Anchor, DateOnly? ActiveFrom, DateOnly? ActiveTo)
    {
        public DateOnly EffectiveFrom => ActiveFrom ?? Anchor;

        public bool IsActiveOn(DateOnly date) =>
            date >= EffectiveFrom && (!ActiveTo.HasValue || date <= ActiveTo.Value);
    }
}
