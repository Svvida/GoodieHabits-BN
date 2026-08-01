using Application.Common.Interfaces;

namespace Application.Quests.Queries.GetHabitsOverview
{
    /// <summary>
    /// Cross-quest analytics for a user: one summary per repeatable quest plus a combined roll-up over a
    /// calendar range. <c>From</c>/<c>To</c> are inclusive and optional (default: the last 30 days).
    /// </summary>
    public record GetHabitsOverviewQuery(
        int UserProfileId,
        DateOnly? From,
        DateOnly? To) : IQuery<GetHabitsOverviewResponse>;
}
