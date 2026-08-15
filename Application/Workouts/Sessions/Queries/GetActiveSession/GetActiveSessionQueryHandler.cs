using Application.Workouts.Sessions.Dtos;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Workouts.Sessions.Queries.GetActiveSession
{
    public class GetActiveSessionQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<GetActiveSessionQuery, WorkoutSessionDto?>
    {
        public async Task<WorkoutSessionDto?> Handle(GetActiveSessionQuery request, CancellationToken cancellationToken)
        {
            var session = await unitOfWork.WorkoutSessions
                .GetActiveAsync(request.UserProfileId, cancellationToken)
                .ConfigureAwait(false);

            return session is null ? null : mapper.Map<WorkoutSessionDto>(session);
        }
    }
}
