using Application.Common.Interfaces;
using Application.Workouts.Sessions.Dtos;

namespace Application.Workouts.Sessions.Commands.FinishSession
{
    public record FinishSessionCommand(int SessionId, int UserProfileId) : ICommand<WorkoutSessionDto>;
}
