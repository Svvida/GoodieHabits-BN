using Domain.Models;

namespace Domain.ValueObjects
{
    /// <summary>
    /// What recording (or undoing) a completion actually did. Returned so the application layer can react —
    /// award badges, notify a level-up — without re-deriving any of it from the entity's state.
    /// </summary>
    /// <param name="Completion">The row that was written, or the pre-existing one when the request was a replay.</param>
    /// <param name="Period">The period it landed in; null for an off-schedule completion.</param>
    /// <param name="WasAlreadyRecorded">True when an idempotency key matched and nothing changed.</param>
    /// <param name="PeriodCompleted">True when this call is what reached the period's target.</param>
    /// <param name="PeriodUncompleted">True when undoing dropped the period back below its target.</param>
    /// <param name="Reward">What was paid out; <see cref="QuestReward.None"/> when nothing was.</param>
    public readonly record struct QuestCompletionResult(
        QuestCompletion Completion,
        QuestOccurrence? Period,
        bool WasAlreadyRecorded,
        bool PeriodCompleted,
        bool PeriodUncompleted,
        QuestReward Reward,
        bool GoalAchieved,
        bool IsFirstEverCompletion);
}
