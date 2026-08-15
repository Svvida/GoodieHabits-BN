using Application.Workouts.Routines.Dtos;

namespace Application.Workouts.Routines.Commands.UpdateRoutine
{
    // Full replacement of the routine *and* its exercise list, matching PUT /finance/transactions/{id}.
    // Whatever array arrives becomes the routine: items left out are removed, and the order is the order.
    // Partially patching an ordered collection was rejected as a bug farm.
    public record UpdateRoutineRequest(
        string Name,
        string? Description = null,
        IReadOnlyList<RoutineExerciseInput>? Exercises = null);
}
