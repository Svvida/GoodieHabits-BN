using Domain.ValueObjects;

namespace Application.Quests.Queries.GetHabitsOverview
{
    /// <param name="Overall">Every repeatable quest's periods pooled together.</param>
    /// <param name="DailyCompletionRate">
    /// Per-calendar-day completion rate across all of the user's habits — the series a streak/heatmap
    /// widget on a dashboard is built from.
    /// </param>
    public record GetHabitsOverviewResponse(
        DateOnly From,
        DateOnly To,
        QuestAnalyticsSummary Overall,
        IReadOnlyList<HabitSummaryDto> Quests,
        IReadOnlyList<DailyCompletionRateDto> DailyCompletionRate);

    public record HabitSummaryDto(
        int QuestId,
        string QuestType,
        string Title,
        string? Emoji,
        QuestAnalyticsSummary Summary);

    public record DailyCompletionRateDto(
        DateOnly Date,
        int CompletedPeriods,
        int EvaluatedPeriods,
        double? CompletionRate);
}
