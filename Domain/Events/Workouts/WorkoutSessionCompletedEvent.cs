namespace Domain.Events.Workouts
{
    /// <summary>
    /// Raised by <c>WorkoutSession.Complete</c>. Nothing consumes it yet — it is the gamification hook the
    /// module ships with so that awarding XP/coins/badges for training later is additive.
    /// <para>
    /// Note the repo has <em>no</em> central dispatch in <c>SaveChanges</c> (ARCHITECTURE §9): whichever handler
    /// completes a session must publish and clear the entity's domain events itself.
    /// </para>
    /// </summary>
    public record WorkoutSessionCompletedEvent(
        int WorkoutSessionId,
        int UserProfileId,
        DateOnly PerformedOn,
        int ExerciseCount,
        int SetCount);
}
