using Application.Common.Interfaces;
using Application.Workouts.Sessions.Dtos;

namespace Application.Workouts.Sessions.Commands.UpdateSession
{
    public record UpdateSessionCommand(
        int SessionId,
        string Name,
        DateOnly PerformedOn,
        string? Note,
        int UserProfileId) : ICommand<WorkoutSessionDto>;
}
