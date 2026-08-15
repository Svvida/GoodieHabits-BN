using Application.Common.Interfaces;
using Application.Workouts.Sessions.Dtos;

namespace Application.Workouts.Sessions.Commands.LogSession
{
    public record LogSessionCommand(
        int SessionId,
        IReadOnlyList<SessionExerciseInput> Exercises,
        int UserProfileId) : ICommand<WorkoutSessionDto>;
}
