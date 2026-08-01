using Application.Common.Interfaces;
using Domain.Enums;

namespace Application.Quests.Queries.GetQuestAnalytics
{
    /// <summary>
    /// Full analytics for one repeatable quest over a calendar range. <c>From</c>/<c>To</c> are inclusive
    /// and optional — when omitted the handler defaults to the last 90 days ending on the user's today.
    /// </summary>
    public record GetQuestAnalyticsQuery(
        int QuestId,
        int UserProfileId,
        DateOnly? From,
        DateOnly? To,
        AnalyticsGranularityEnum Granularity) : IQuery<GetQuestAnalyticsResponse>;
}
