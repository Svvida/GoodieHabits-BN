using Application.Common.Interfaces;
using Application.Workouts.Exercises.Dtos;
using Domain.Enums;

namespace Application.Workouts.Exercises.Queries.GetExercises
{
    /// <summary>
    /// The library as one user sees it: seeded system exercises plus their own. Every filter is optional;
    /// archived rows are excluded unless asked for.
    /// </summary>
    public record GetExercisesQuery(
        int UserProfileId,
        MuscleGroupEnum? MuscleGroup,
        ExerciseMetricEnum? MetricType,
        EquipmentEnum? Equipment,
        string? Search,
        bool IncludeArchived) : IQuery<IEnumerable<ExerciseDto>>;
}
