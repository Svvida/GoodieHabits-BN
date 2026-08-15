using Application.Common.Interfaces;
using Application.Workouts.Routines.Dtos;

namespace Application.Workouts.Routines.Commands.UpdateRoutine
{
    public record UpdateRoutineCommand(
        int RoutineId,
        string Name,
        string? Description,
        IReadOnlyList<RoutineExerciseInput> Exercises,
        int UserProfileId) : ICommand<WorkoutRoutineDto>;
}
