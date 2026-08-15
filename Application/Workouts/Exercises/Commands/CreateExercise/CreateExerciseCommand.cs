using Application.Common.Interfaces;
using Application.Workouts.Exercises.Dtos;
using Domain.Enums;

namespace Application.Workouts.Exercises.Commands.CreateExercise
{
    public record CreateExerciseCommand(
        string Name,
        ExerciseMetricEnum MetricType,
        MuscleGroupEnum MuscleGroup,
        EquipmentEnum Equipment,
        string? Note,
        int UserProfileId) : ICommand<ExerciseDto>;
}
