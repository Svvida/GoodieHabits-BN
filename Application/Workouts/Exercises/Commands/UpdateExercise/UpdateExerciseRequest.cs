using Domain.Enums;

namespace Application.Workouts.Exercises.Commands.UpdateExercise
{
    // Full replacement, like PUT /finance/transactions/{id}: every field is sent, omitted ones are not
    // "leave as is". Changing MetricType is allowed — past sessions snapshotted the metric they were logged
    // under, so history is unaffected, but routine targets set under the old metric may stop making sense.
    public record UpdateExerciseRequest(
        string Name,
        ExerciseMetricEnum MetricType,
        MuscleGroupEnum MuscleGroup = MuscleGroupEnum.Other,
        EquipmentEnum Equipment = EquipmentEnum.None,
        string? Note = null);
}
