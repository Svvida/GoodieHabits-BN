using Application.Common.Interfaces;
using Application.Workouts.Routines.Dtos;

namespace Application.Workouts.Routines.Queries.GetRoutineById
{
    public record GetRoutineByIdQuery(int RoutineId, int UserProfileId) : IQuery<WorkoutRoutineDto>;
}
