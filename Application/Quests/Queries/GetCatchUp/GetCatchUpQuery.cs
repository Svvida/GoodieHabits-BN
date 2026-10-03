using Application.Common.Interfaces;
using Application.Quests.Dtos;

namespace Application.Quests.Queries.GetCatchUp
{
    /// <summary>
    /// The periods a forgotten tap would still fix. People do the habit and forget to record it; this is the
    /// list the app shows on open so they can put it right, and it is the reason completions carry the day
    /// they count for rather than being inferred from "now".
    /// </summary>
    /// <param name="IncludeCompleted">
    /// Also return periods in the window that are already done. Off by default, so an empty
    /// <see cref="GetCatchUpResponse.Days"/> keeps meaning "nothing to ask about" and the card can simply
    /// hide itself. Turn it on to let the user undo a catch-up tap made in an earlier session — otherwise
    /// a mistaken one is unreachable, because completing a period removes it from the default list.
    /// </param>
    public record GetCatchUpQuery(int UserProfileId, bool IncludeCompleted = false) : IQuery<GetCatchUpResponse>;

    /// <param name="GraceDays">How many days back a completion may be dated, so the client can label the card.</param>
    /// <param name="Days">Oldest first. Empty means there is nothing to ask about — hide the card entirely.</param>
    public record GetCatchUpResponse(int GraceDays, IReadOnlyList<CatchUpDayDto> Days);

    public record CatchUpDayDto(DateOnly Date, IReadOnlyList<CatchUpQuestDto> Quests);

    /// <param name="Outcome">"Missed" or "Partial" — plus "Completed" when <c>includeCompleted</c> is set.</param>
    /// <param name="Completions">That period's taps, carrying the ids needed to undo one.</param>
    public record CatchUpQuestDto(
        int QuestId,
        string Title,
        string? Emoji,
        DateOnly PeriodStart,
        DateOnly PeriodEnd,
        decimal Progress,
        decimal Target,
        string Outcome,
        IReadOnlyList<PeriodCompletionDto> Completions);
}
