using Domain.Enums;

namespace Application.Workouts.Routines.Dtos
{
    /// <summary>
    /// A planned exercise inside a routine. <see cref="ExerciseName"/> / <see cref="MetricType"/> are read from
    /// the library row (a routine always points at a live exercise — deleting one is blocked while a routine
    /// uses it), unlike a performed session, which snapshots them.
    /// </summary>
    public class WorkoutRoutineExerciseDto
    {
        public int Id { get; set; }
        public int ExerciseId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public ExerciseMetricEnum MetricType { get; set; }
        public MuscleGroupEnum MuscleGroup { get; set; }

        /// <summary>0-based position. The order of the array is the order of the workout.</summary>
        public int Order { get; set; }

        public int? TargetSets { get; set; }
        public int? TargetReps { get; set; }
        public decimal? TargetWeight { get; set; }
        public int? TargetDurationSeconds { get; set; }
        public decimal? TargetDistance { get; set; }
        public int? RestSeconds { get; set; }
        public string? Note { get; set; }
    }
}
