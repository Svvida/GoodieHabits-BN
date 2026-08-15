using Application.Workouts.Routines.Dtos;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Workouts.Routines.Queries.GetRoutines
{
    public class GetRoutinesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<GetRoutinesQuery, IEnumerable<WorkoutRoutineDto>>
    {
        public async Task<IEnumerable<WorkoutRoutineDto>> Handle(GetRoutinesQuery request, CancellationToken cancellationToken)
        {
            var routines = await unitOfWork.WorkoutRoutines
                .GetUserRoutinesAsync(request.UserProfileId, request.IncludeArchived, cancellationToken)
                .ConfigureAwait(false);

            // Exercises come back with the list on purpose: this is the "pick a routine to start" screen, and a
            // handful of routines per user makes a second round-trip per card pure waste.
            return routines.Select(mapper.Map<WorkoutRoutineDto>).ToList();
        }
    }
}
