using Application.Common.Interfaces;
using Application.Workouts.Sessions.Dtos;

namespace Application.Workouts.Sessions.Commands.AbandonSession
{
    public record AbandonSessionCommand(int SessionId, int UserProfileId) : ICommand<WorkoutSessionDto>;
}
