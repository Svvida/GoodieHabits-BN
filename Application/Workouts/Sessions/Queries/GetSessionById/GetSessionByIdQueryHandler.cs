using Application.Workouts.Sessions.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Workouts.Sessions.Queries.GetSessionById
{
    public class GetSessionByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<GetSessionByIdQuery, WorkoutSessionDto>
    {
        public async Task<WorkoutSessionDto> Handle(GetSessionByIdQuery request, CancellationToken cancellationToken)
        {
            var session = await unitOfWork.WorkoutSessions
                .GetOwnedByIdAsync(request.SessionId, request.UserProfileId, true, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Session with ID {request.SessionId} not found.");

            return mapper.Map<WorkoutSessionDto>(session);
        }
    }
}
