using Application.Workouts.Routines.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Workouts.Routines.Queries.GetRoutineById
{
    public class GetRoutineByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<GetRoutineByIdQuery, WorkoutRoutineDto>
    {
        public async Task<WorkoutRoutineDto> Handle(GetRoutineByIdQuery request, CancellationToken cancellationToken)
        {
            var routine = await unitOfWork.WorkoutRoutines
                .GetOwnedByIdAsync(request.RoutineId, request.UserProfileId, true, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Routine with ID {request.RoutineId} not found.");

            return mapper.Map<WorkoutRoutineDto>(routine);
        }
    }
}
