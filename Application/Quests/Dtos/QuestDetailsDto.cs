using Application.QuestLabels.Dtos;

namespace Application.Quests.Dtos
{
    /// <summary>
    /// One quest, whatever its schedule. This used to be a polymorphic hierarchy with one subtype per
    /// <c>QuestType</c>; the recurrence now lives in <see cref="Schedule"/>, so a single shape serves every
    /// quest and clients no longer branch on a discriminator to read the same fields.
    /// </summary>
    public record QuestDetailsDto
    {
        public int Id { get; init; }
        public string Title { get; init; } = null!;
        public string? Description { get; init; }
        public DateOnly? StartDate { get; init; }
        public DateOnly? EndDate { get; init; }
        public string? Emoji { get; init; }
        public string? Priority { get; init; }
        public string? Difficulty { get; init; }
        public TimeOnly? ScheduledTime { get; init; }
        public int? DurationMinutes { get; init; }

        public QuestScheduleDto Schedule { get; init; } = null!;
        public QuestTargetDto Target { get; init; } = null!;

        /// <summary>
        /// Derived from the period the user is currently in — there is no stored completed flag any more.
        /// </summary>
        public bool IsCompleted { get; init; }

        /// <summary>
        /// Everything a list row needs to render progress and urgency. Null when the quest is not due today.
        /// </summary>
        public CurrentPeriodDto? CurrentPeriod { get; init; }

        public DateTime? LastCompletedAt { get; init; }
        public RepeatableQuestStatisticsDto? Statistics { get; init; }
        public ICollection<QuestLabelDto> Labels { get; init; } = [];

        /// <summary>
        /// The nearest old <c>QuestType</c> for this schedule. Written only for the deprecated compatibility
        /// routes, so an installed app build keeps working through one release; new clients read
        /// <see cref="Schedule"/>.
        /// </summary>
        public string? LegacyQuestType { get; init; }
    }

    /// <param name="Unit">"None", "Day", "Week", "Month" or "Year" — what one accountability period is.</param>
    /// <param name="Weekdays">Day schedules only: which weekdays are due. Null means every day.</param>
    public record QuestScheduleDto(
        string Unit,
        int Interval,
        IReadOnlyList<string>? Weekdays,
        int? MonthWindowStartDay,
        int? MonthWindowEndDay,
        int? YearWindowStart,
        int? YearWindowEnd);

    /// <param name="Unit">Null means the target is counted in plain repetitions ("twice").</param>
    /// <param name="MaxCompletionsPerDay">How many completions one day may contribute. Null means no cap.</param>
    public record QuestTargetDto(
        decimal Amount,
        string? Unit,
        string Mode,
        int? MaxCompletionsPerDay);

    /// <param name="Outcome">"Completed", "Missed", "Partial", "Pending" or "Skipped".</param>
    /// <param name="RemainingDays">Days left in the period, today included.</param>
    /// <param name="IsAtRisk">
    /// True when finishing the period is still possible but no longer comfortable — more completions are
    /// needed than there are days left to make them in. This is what drives "2 more workouts, 2 days left".
    /// </param>
    /// <param name="TodayProgress">
    /// How much of <paramref name="Progress"/> was recorded on the user's local today. Served rather than
    /// left to the client because "today" depends on the profile timezone, which only the server resolves.
    /// </param>
    /// <param name="CanCompleteToday">
    /// False once the per-day cap is reached. The button state, computed here so the client never
    /// re-implements the rule — and so the answer survives an app restart, which a local flag does not.
    /// </param>
    /// <param name="Completions">
    /// This period's taps, oldest first. Carries the ids that
    /// <c>DELETE /quests/{id}/completions/{completionId}</c> needs: without them undo only worked inside
    /// the session that recorded the tap.
    /// </param>
    public record CurrentPeriodDto(
        DateOnly Start,
        DateOnly End,
        decimal Progress,
        decimal Target,
        decimal Remaining,
        string Outcome,
        int RemainingDays,
        bool IsAtRisk,
        decimal TodayProgress,
        bool CanCompleteToday,
        IReadOnlyList<PeriodCompletionDto> Completions);

    /// <summary>One recorded tap inside the current period — enough to undo it or to show when it happened.</summary>
    public record PeriodCompletionDto(int Id, DateOnly CompletedOn, decimal Amount);
}
