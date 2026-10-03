namespace Application.Quests.Dtos
{
    /// <summary>
    /// The recurrence half of a create/update request. One shape for every kind of quest — the five
    /// per-type request bodies it replaces differed only in which of these fields they carried.
    /// </summary>
    public record QuestScheduleRequest
    {
        /// <summary>"None", "Day", "Week", "Month" or "Year".</summary>
        public string Unit { get; init; } = "Day";

        /// <summary>Every N units, counted from the quest's start date. 1 unless you want "every other day".</summary>
        public int Interval { get; init; } = 1;

        /// <summary>Day schedules only. Omit for every day; this is how the old Weekly quest is expressed.</summary>
        public HashSet<string>? Weekdays { get; init; }

        /// <summary>Month schedules only: the days of the month the quest is due between, inclusive.</summary>
        public int? MonthWindowStartDay { get; init; }
        public int? MonthWindowEndDay { get; init; }

        /// <summary>Year schedules only, as MMDD. An end before the start wraps past new year (a winter season).</summary>
        public int? YearWindowStart { get; init; }
        public int? YearWindowEnd { get; init; }
    }

    /// <summary>The "how much counts as done" half of a create/update request.</summary>
    public record QuestTargetRequest
    {
        /// <summary>Defaults to 1, which reproduces the old tick-it-once behaviour exactly.</summary>
        public decimal Amount { get; init; } = 1m;

        /// <summary>Omit to count plain repetitions; otherwise a label such as "L", "pages" or "min".</summary>
        public string? Unit { get; init; }

        /// <summary>"AtLeast" to build a habit. "AtMost" is reserved and currently rejected.</summary>
        public string Mode { get; init; } = "AtLeast";

        /// <summary>
        /// Caps how many completions a single day may contribute, so "twice a week" cannot be finished twice
        /// on a Monday. Meaningless for Day schedules, where the period already is a day.
        /// </summary>
        public int? MaxCompletionsPerDay { get; init; }
    }
}
