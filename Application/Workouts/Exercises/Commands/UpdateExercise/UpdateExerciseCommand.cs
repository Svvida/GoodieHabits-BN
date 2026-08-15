using Application.Common.Interfaces;
using Application.Workouts.Exercises.Dtos;
using Domain.Enums;

namespace Application.Workouts.Exercises.Commands.UpdateExercise
{
    public record UpdateExerciseCommand(
        int ExerciseId,
        string Name,
        ExerciseMetricEnum MetricType,
        MuscleGroupEnum MuscleGroup,
        EquipmentEnum Equipment,
        string? Note,
        int UserProfileId) : ICommand<ExerciseDto>;
}
