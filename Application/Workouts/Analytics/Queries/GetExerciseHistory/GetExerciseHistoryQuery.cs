using Application.Common.Interfaces;
using Application.Workouts.Analytics.Dtos;

namespace Application.Workouts.Analytics.Queries.GetExerciseHistory
{
    public record GetExerciseHistoryQuery(int UserProfileId, int ExerciseId, DateOnly From, DateOnly To)
        : IQuery<ExerciseHistoryDto>;
}
