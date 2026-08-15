using Application.Common.Interfaces;

namespace Application.Workouts.Routines.Commands.DeleteRoutine
{
    public record DeleteRoutineCommand(int RoutineId, int UserProfileId) : ICommand;
}
