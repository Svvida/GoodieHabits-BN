using Application.Common.Interfaces;
using Application.Workouts.Sessions.Dtos;

namespace Application.Workouts.Sessions.Commands.AddSessionExercise
{
    /// <summary>
    /// Appends an exercise to a session mid-workout. The request body is a
    /// <see cref="SessionExerciseInput"/> — the same shape the bulk log uses — so an exercise can arrive with
    /// its sets already filled in, or empty and be logged set by set.
    /// </summary>
    public record AddSessionExerciseCommand(
        int SessionId,
        SessionExerciseInput Exercise,
        int UserProfileId) : ICommand<WorkoutSessionDto>;
}
