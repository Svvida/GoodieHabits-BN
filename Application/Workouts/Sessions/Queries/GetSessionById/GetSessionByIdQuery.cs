using Application.Common.Interfaces;
using Application.Workouts.Sessions.Dtos;

namespace Application.Workouts.Sessions.Queries.GetSessionById
{
    public record GetSessionByIdQuery(int SessionId, int UserProfileId) : IQuery<WorkoutSessionDto>;
}
