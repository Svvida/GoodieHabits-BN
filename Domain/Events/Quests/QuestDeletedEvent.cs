namespace Domain.Events.Quests
{
    /// <summary>
    /// Raised when a quest is deleted, so the profile's denormalized counters can be adjusted.
    /// <para>
    /// There is no "was it completed" flag any more: completion is derived from the period the user is
    /// currently in, and <c>CurrentlyCompletedExistingQuests</c> is recomputed by the maintenance pass, so
    /// the handler no longer needs to be told.
    /// </para>
    /// </summary>
    public record QuestDeletedEvent(
        int QuestId,
        int UserProfileId,
        bool IsQuestEverCompleted);
}
