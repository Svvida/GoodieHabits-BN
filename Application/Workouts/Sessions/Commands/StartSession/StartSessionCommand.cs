using Application.Common.Interfaces;
using Application.Workouts.Sessions.Dtos;

namespace Application.Workouts.Sessions.Commands.StartSession
{
    public record StartSessionCommand(
        int? RoutineId,
        string? Name,
        DateOnly? PerformedOn,
        string? Note,
        int UserProfileId) : ICommand<WorkoutSessionDto>;
}
