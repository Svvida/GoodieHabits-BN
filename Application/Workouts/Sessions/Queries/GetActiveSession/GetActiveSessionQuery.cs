using Application.Common.Interfaces;
using Application.Workouts.Sessions.Dtos;

namespace Application.Workouts.Sessions.Queries.GetActiveSession
{
    /// <summary>
    /// The session currently being logged, or <c>null</c>. "No active session" is an ordinary state, not an
    /// error, so the controller answers <b>204 No Content</b> rather than a 404.
    /// </summary>
    public record GetActiveSessionQuery(int UserProfileId) : IQuery<WorkoutSessionDto?>;
}
