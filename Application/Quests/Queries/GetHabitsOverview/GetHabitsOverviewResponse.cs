using Domain.ValueObjects;

namespace Application.Quests.Queries.GetHabitsOverview
{
    /// <param name="Overall">Every repeating quest's periods pooled together.</param>
    /// <param name="DailyCompletionRate">
    /// Per-calendar-day completion rate across the user's day-scheduled habits only — the series a streak or
    /// heatmap widget is built from.
    /// </param>
    /// <param name="Periodic">
    /// The same roll-up for week, month and year periods, which have no meaningful place on a per-day axis.
    /// Previously these were smeared across every day they covered, so one missed weekly target painted
    /// seven days red.
    /// </param>
    public record GetHabitsOverviewResponse(
        DateOnly From,
        DateOnly To,
        QuestAnalyticsSummary Overall,
        IReadOnlyList<HabitSummaryDto> Quests,
        IReadOnlyList<DailyCompletionRateDto> DailyCompletionRate,
        QuestAnalyticsSummary Periodic);

    /// <param name="StreakUnit">"Day", "Week", "Month" or "Year" — what this quest's streak counts in.</param>
    public record HabitSummaryDto(
        int QuestId,
        string StreakUnit,
        string Title,
        string? Emoji,
        QuestAnalyticsSummary Summary);

    public record DailyCompletionRateDto(
        DateOnly Date,
        int CompletedPeriods,
        int EvaluatedPeriods,
        double? CompletionRate);
}
