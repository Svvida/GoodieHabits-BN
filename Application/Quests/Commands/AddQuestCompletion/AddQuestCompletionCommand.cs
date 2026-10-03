using Application.Common.Interfaces;
using Application.Quests.Dtos;

namespace Application.Quests.Commands.AddQuestCompletion
{
    /// <summary>
    /// Records one act of doing the quest. Replaces the old <c>PATCH .../completion { isCompleted }</c>
    /// toggle: a boolean cannot express "I did it twice today", and it had no way to say which day a
    /// completion belonged to, which is why forgotten taps could never be backfilled.
    /// </summary>
    /// <param name="Amount">Defaults to 1. For a measured target this is litres, pages, minutes.</param>
    /// <param name="CompletedOn">
    /// The local day this counts for. Defaults to today; may be up to
    /// <see cref="Domain.Models.Quest.BackfillGraceDays"/> days back, which is the catch-up window.
    /// </param>
    /// <param name="ClientRequestId">
    /// Idempotency key. Send a stable one per user action so a retry or a double tap cannot record twice.
    /// </param>
    public record AddQuestCompletionCommand(
        int QuestId,
        int UserProfileId,
        decimal? Amount = null,
        DateOnly? CompletedOn = null,
        Guid? ClientRequestId = null,
        string? Note = null) : ICommand<QuestCompletionResponse>, ICurrentUserQuestCommand;

    /// <summary>Body of <c>POST /api/quests/{id}/completions</c>.</summary>
    public record AddQuestCompletionRequest
    {
        public decimal? Amount { get; init; }
        public DateOnly? CompletedOn { get; init; }
        public Guid? ClientRequestId { get; init; }
        public string? Note { get; init; }
    }

    /// <param name="Quest">The quest as it now stands, so the client can re-render the row from one response.</param>
    /// <param name="WasAlreadyRecorded">True when the idempotency key matched and nothing new was written.</param>
    public record QuestCompletionResponse(
        QuestDetailsDto Quest,
        QuestCompletionDto Completion,
        bool WasAlreadyRecorded,
        bool PeriodCompleted,
        int XpAwarded,
        int CoinsAwarded);

    public record QuestCompletionDto(
        int Id,
        int QuestId,
        int? OccurrenceId,
        DateOnly CompletedOn,
        DateTime CompletedAt,
        TimeOnly? LocalTime,
        decimal Amount,
        bool IsBackfilled,
        bool IsOffSchedule,
        string Source,
        string? Note);
}
