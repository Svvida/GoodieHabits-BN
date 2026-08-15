using Domain.Enums;

namespace Application.Workouts.Exercises.Dtos
{
    /// <summary>
    /// A library exercise as the client sees it. <see cref="MetricType"/> is the field the UI renders from —
    /// it decides which inputs a set needs — and <see cref="IsSystem"/> tells the client the row is read-only.
    /// </summary>
    public class ExerciseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ExerciseMetricEnum MetricType { get; set; }
        public MuscleGroupEnum MuscleGroup { get; set; }
        public EquipmentEnum Equipment { get; set; }
        public string? Note { get; set; }

        /// <summary>Seeded and shared by everyone. Cannot be edited, archived or deleted.</summary>
        public bool IsSystem { get; set; }

        /// <summary>Hidden from the picker, but still present in routines and past sessions.</summary>
        public bool IsArchived { get; set; }
    }
}
