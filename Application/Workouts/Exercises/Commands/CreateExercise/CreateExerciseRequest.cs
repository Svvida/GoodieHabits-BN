using Domain.Enums;

namespace Application.Workouts.Exercises.Commands.CreateExercise
{
    // MetricType decides which measurements a set of this exercise must carry, so it is the one field worth
    // getting right on the client. It is a value type: an omitted metricType deserializes to "Reps" (0).
    public record CreateExerciseRequest(
        string Name,
        ExerciseMetricEnum MetricType,
        MuscleGroupEnum MuscleGroup = MuscleGroupEnum.Other,
        EquipmentEnum Equipment = EquipmentEnum.None,
        string? Note = null);
}
