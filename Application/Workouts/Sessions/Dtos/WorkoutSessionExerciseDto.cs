using Domain.Enums;

namespace Application.Workouts.Sessions.Dtos
{
    /// <summary>
    /// One exercise as it appeared in a performed session.
    /// <para>
    /// ⚠️ <see cref="ExerciseName"/> and <see cref="MetricType"/> are <b>snapshots</b> taken when the exercise
    /// was added, not live reads of the library. Renaming an exercise, or changing its metric, never rewrites
    /// history. <see cref="ExerciseId"/> is null once the library row has been deleted — render from the
    /// snapshot, and treat the id as "can I link to the library entry", nothing more.
    /// </para>
    /// </summary>
    public class WorkoutSessionExerciseDto
    {
        public int Id { get; set; }
        public int? ExerciseId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public ExerciseMetricEnum MetricType { get; set; }
        public int Order { get; set; }

        public int? TargetSets { get; set; }
        public int? TargetReps { get; set; }
        public decimal? TargetWeight { get; set; }
        public int? TargetDurationSeconds { get; set; }
        public decimal? TargetDistance { get; set; }
        public int? RestSeconds { get; set; }
        public string? Note { get; set; }

        public List<WorkoutSetDto> Sets { get; set; } = [];
    }
}
