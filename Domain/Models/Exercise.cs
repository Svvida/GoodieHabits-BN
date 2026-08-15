using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Models
{
    /// <summary>
    /// One movement in the exercise library, discriminated by <see cref="MetricType"/> rather than by a class
    /// hierarchy — the same shape as <see cref="Quest"/> and <see cref="FinanceTransaction"/>. The metric
    /// declares which of a logged set's nullable measurement columns are meaningful, which is what buys cardio
    /// and isometrics for free.
    /// <para>
    /// System exercises (<see cref="IsSystem"/>) are seeded globally and have no owner
    /// (<see cref="UserProfileId"/> is null); user exercises are owned by a single profile. Exactly the
    /// <see cref="FinanceCategory"/> arrangement, including its identity hazard — see the module docs.
    /// </para>
    /// </summary>
    public class Exercise : EntityBase
    {
        public const int NameMaxLength = 100;
        public const int NoteMaxLength = 500;

        public int Id { get; set; }
        public int? UserProfileId { get; private set; }   // null => system/seeded (global)
        public string Name { get; private set; } = null!;
        public ExerciseMetricEnum MetricType { get; private set; }
        public MuscleGroupEnum MuscleGroup { get; private set; }
        public EquipmentEnum Equipment { get; private set; }
        public string? Note { get; private set; }
        public bool IsSystem { get; private set; }

        /// <summary>
        /// Retires the exercise from the picker without deleting it. Archived exercises stay in the routines
        /// and session history that already reference them — deleting is blocked while a routine uses it, so
        /// this is the escape hatch for "I don't do this any more" that never costs the user their history.
        /// </summary>
        public bool IsArchived { get; private set; }

        public UserProfile? UserProfile { get; set; }
        public ICollection<WorkoutRoutineExercise> RoutineExercises { get; set; } = [];
        public ICollection<WorkoutSessionExercise> SessionExercises { get; set; } = [];

        protected Exercise() { }

        private Exercise(
            int? userProfileId,
            string name,
            ExerciseMetricEnum metricType,
            MuscleGroupEnum muscleGroup,
            EquipmentEnum equipment,
            string? note,
            bool isSystem)
        {
            ValidateName(name);
            ValidateNote(note);

            UserProfileId = userProfileId;
            Name = name.Trim();
            MetricType = metricType;
            MuscleGroup = muscleGroup;
            Equipment = equipment;
            Note = note?.Trim();
            IsSystem = isSystem;
        }

        public static Exercise Create(
            int userProfileId,
            string name,
            ExerciseMetricEnum metricType,
            MuscleGroupEnum muscleGroup = MuscleGroupEnum.Other,
            EquipmentEnum equipment = EquipmentEnum.None,
            string? note = null)
        {
            if (userProfileId <= 0)
                throw new InvalidArgumentException("UserProfileId must be greater than zero.");

            return new Exercise(userProfileId, name, metricType, muscleGroup, equipment, note, isSystem: false);
        }

        /// <summary>Factory for seeding global system exercises. Id is explicit for stable seed data.</summary>
        public static Exercise CreateSystem(
            int id,
            string name,
            ExerciseMetricEnum metricType,
            MuscleGroupEnum muscleGroup = MuscleGroupEnum.Other,
            EquipmentEnum equipment = EquipmentEnum.None,
            string? note = null)
            => new(null, name, metricType, muscleGroup, equipment, note, isSystem: true) { Id = id };

        public void Rename(string name)
        {
            ValidateName(name);
            Name = name.Trim();
        }

        public void UpdateNote(string? note)
        {
            ValidateNote(note);
            Note = note?.Trim();
        }

        public void UpdateClassification(MuscleGroupEnum muscleGroup, EquipmentEnum equipment)
        {
            MuscleGroup = muscleGroup;
            Equipment = equipment;
        }

        /// <summary>
        /// Changing the metric is allowed. Past sessions are unaffected because
        /// <see cref="WorkoutSessionExercise"/> snapshots the metric it was logged under; targets on routines
        /// that used the old metric may stop making sense, which is the caller's problem to surface.
        /// </summary>
        public void ChangeMetricType(ExerciseMetricEnum metricType) => MetricType = metricType;

        public void SetArchived(bool isArchived) => IsArchived = isArchived;

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidArgumentException("Exercise name cannot be null or whitespace.");
            if (name.Trim().Length > NameMaxLength)
                throw new InvalidArgumentException($"Exercise name cannot exceed {NameMaxLength} characters.");
        }

        private static void ValidateNote(string? note)
        {
            if (note is not null && note.Trim().Length > NoteMaxLength)
                throw new InvalidArgumentException($"Note cannot exceed {NoteMaxLength} characters.");
        }
    }
}
