namespace Domain.Enums
{
    public enum BadgeTriggerEnum
    {
        QuestCreated,
        QuestCompleted,
        GoalCreated,

        /// <summary>
        /// Raised when a training session is finished. The trigger and its domain event exist from the first
        /// release of the workouts module; no <c>IBadgeAwardingStrategy</c> consumes it yet — awarding XP/coins
        /// for training is a deliberate backlog item, and the hook is here so adding it later needs no
        /// migration and no pass over session history.
        /// </summary>
        WorkoutSessionCompleted,
    }
}
