using Application.Common.Interfaces;
using Application.Workouts.Routines.Dtos;

namespace Application.Workouts.Routines.Commands.CreateRoutine
{
    public record CreateRoutineCommand(
        string Name,
        string? Description,
        IReadOnlyList<RoutineExerciseInput> Exercises,
        int UserProfileId) : ICommand<WorkoutRoutineDto>;
}
