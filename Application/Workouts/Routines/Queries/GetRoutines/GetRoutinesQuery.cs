using Application.Common.Interfaces;
using Application.Workouts.Routines.Dtos;

namespace Application.Workouts.Routines.Queries.GetRoutines
{
    public record GetRoutinesQuery(int UserProfileId, bool IncludeArchived)
        : IQuery<IEnumerable<WorkoutRoutineDto>>;
}
