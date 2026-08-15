using Application.Common.Interfaces;
using Application.Workouts.Sessions.Dtos;

namespace Application.Workouts.Sessions.Commands.DeleteSet
{
    public record DeleteSetCommand(int SessionId, int EntryId, int SetId, int UserProfileId)
        : ICommand<WorkoutSessionDto>;
}
