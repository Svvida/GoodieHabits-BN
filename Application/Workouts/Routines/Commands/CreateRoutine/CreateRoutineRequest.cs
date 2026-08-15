using Application.Workouts.Routines.Dtos;

namespace Application.Workouts.Routines.Commands.CreateRoutine
{
    // The order of Exercises is the order of the workout. An empty list is legal — a routine under
    // construction is not an error.
    public record CreateRoutineRequest(
        string Name,
        string? Description = null,
        IReadOnlyList<RoutineExerciseInput>? Exercises = null);
}
