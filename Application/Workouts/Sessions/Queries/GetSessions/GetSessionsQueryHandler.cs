using Application.Common.Dtos;
using Application.Workouts.Sessions.Dtos;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Workouts.Sessions.Queries.GetSessions
{
    /// <summary>
    /// Training history, newest first. The rows come back without their exercise tree but <em>with</em> totals:
    /// the repository loads the sets to fold them, and shipping the whole tree for a list nobody expands is
    /// waste the client would have to skip past anyway.
    /// </summary>
    public class GetSessionsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<GetSessionsQuery, PagedResult<WorkoutSessionSummaryDto>>
    {
        public async Task<PagedResult<WorkoutSessionSummaryDto>> Handle(
            GetSessionsQuery request, CancellationToken cancellationToken)
        {
            var (items, totalCount) = await unitOfWork.WorkoutSessions
                .GetUserSessionsAsync(
                    request.UserProfileId,
                    request.From,
                    request.To,
                    request.Status,
                    request.Page,
                    request.PageSize,
                    cancellationToken)
                .ConfigureAwait(false);

            var dtos = items.Select(mapper.Map<WorkoutSessionSummaryDto>).ToList();

            return new PagedResult<WorkoutSessionSummaryDto>(dtos, request.Page, request.PageSize, totalCount);
        }
    }
}
