using Application.Common.Interfaces;
using Application.Workouts.Analytics.Dtos;

namespace Application.Workouts.Analytics.Queries.GetWorkoutSummary
{
    public record GetWorkoutSummaryQuery(int UserProfileId, DateOnly From, DateOnly To)
        : IQuery<WorkoutSummaryDto>;
}
