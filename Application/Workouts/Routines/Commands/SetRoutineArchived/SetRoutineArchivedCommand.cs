using Application.Common.Interfaces;
using Application.Workouts.Routines.Dtos;

namespace Application.Workouts.Routines.Commands.SetRoutineArchived
{
    public record SetRoutineArchivedCommand(int RoutineId, bool IsArchived, int UserProfileId)
        : ICommand<WorkoutRoutineDto>;
}
