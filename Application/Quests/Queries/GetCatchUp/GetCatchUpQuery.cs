using Application.Common.Interfaces;

namespace Application.Quests.Queries.GetCatchUp
{
    /// <summary>
    /// The periods a forgotten tap would still fix. People do the habit and forget to record it; this is the
    /// list the app shows on open so they can put it right, and it is the reason completions carry the day
    /// they count for rather than being inferred from "now".
    /// </summary>
    public record GetCatchUpQuery(int UserProfileId) : IQuery<GetCatchUpResponse>;

    /// <param name="GraceDays">How many days back a completion may be dated, so the client can label the card.</param>
    /// <param name="Days">Oldest first. Empty means there is nothing to ask about — hide the card entirely.</param>
    public record GetCatchUpResponse(int GraceDays, IReadOnlyList<CatchUpDayDto> Days);

    public record CatchUpDayDto(DateOnly Date, IReadOnlyList<CatchUpQuestDto> Quests);

    /// <param name="Outcome">"Missed" or "Partial" — a completed period is never offered.</param>
    public record CatchUpQuestDto(
        int QuestId,
        string Title,
        string? Emoji,
        DateOnly PeriodStart,
        DateOnly PeriodEnd,
        decimal Progress,
        decimal Target,
        string Outcome);
}
