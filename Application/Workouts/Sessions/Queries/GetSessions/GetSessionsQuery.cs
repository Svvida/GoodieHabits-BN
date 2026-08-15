using Application.Common.Dtos;
using Application.Common.Interfaces;
using Application.Workouts.Sessions.Dtos;
using Domain.Enums;

namespace Application.Workouts.Sessions.Queries.GetSessions
{
    public record GetSessionsQuery(
        int UserProfileId,
        DateOnly? From,
        DateOnly? To,
        WorkoutSessionStatusEnum? Status,
        int Page,
        int PageSize) : IQuery<PagedResult<WorkoutSessionSummaryDto>>;
}
