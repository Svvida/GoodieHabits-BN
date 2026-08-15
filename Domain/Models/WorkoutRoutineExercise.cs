using Domain.Common;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Models
{
    /// <summary>
    /// One planned exercise inside a <see cref="WorkoutRoutine"/>: which movement, in what position, and what
    /// the user is aiming for. Every target is optional — a routine that only fixes the order and lets the
    /// numbers float is a legitimate way to train.
    /// </summary>
    public class WorkoutRoutineExercise : EntityBase
    {
        public const int NoteMaxLength = 250;

        public int Id { get; set; }
        public int WorkoutRoutineId { get; private set; }
        public int ExerciseId { get; private set; }

        /// <summary>Position in the routine, 0-based and contiguous. Maintained by <c>WorkoutRoutine.ReplaceExercises</c>.</summary>
        public int Order { get; private set; }

        public int? TargetSets { get; private set; }
        public int? TargetReps { get; private set; }
        public decimal? TargetWeight { get; private set; }
        public int? TargetDurationSeconds { get; private set; }
        public decimal? TargetDistance { get; private set; }
        public int? RestSeconds { get; private set; }
        public string? Note { get; private set; }

        public WorkoutRoutine WorkoutRoutine { get; set; } = null!;
        public Exercise Exercise { get; set; } = null!;

        protected WorkoutRoutineExercise() { }

        private WorkoutRoutineExercise(
            int exerciseId,
            int order,
            int? targetSets,
            int? targetReps,
            decimal? targetWeight,
            int? targetDurationSeconds,
            decimal? targetDistance,
            int? restSeconds,
            string? note)
        {
            if (exerciseId <= 0)
                throw new InvalidArgumentException("ExerciseId must be greater than zero.");
            if (order < 0)
                throw new InvalidArgumentException("Order cannot be negative.");

            ValidateRange(targetSets, 1, WorkoutLimits.MaxSets, nameof(TargetSets));
            ValidateRange(targetReps, 1, WorkoutLimits.MaxReps, nameof(TargetReps));
            ValidateRange(targetDurationSeconds, 1, WorkoutLimits.MaxDurationSeconds, nameof(TargetDurationSeconds));
            ValidateRange(restSeconds, 0, WorkoutLimits.MaxRestSeconds, nameof(RestSeconds));
            ValidateRange(targetWeight, 0m, WorkoutLimits.MaxWeight, nameof(TargetWeight));
            ValidateRange(targetDistance, 0m, WorkoutLimits.MaxDistance, nameof(TargetDistance));
            ValidateNote(note);

            ExerciseId = exerciseId;
            Order = order;
            TargetSets = targetSets;
            TargetReps = targetReps;
            TargetWeight = targetWeight;
            TargetDurationSeconds = targetDurationSeconds;
            TargetDistance = targetDistance;
            RestSeconds = restSeconds;
            Note = note?.Trim();
        }

        public static WorkoutRoutineExercise Create(
            int exerciseId,
            int order = 0,
            int? targetSets = null,
            int? targetReps = null,
            decimal? targetWeight = null,
            int? targetDurationSeconds = null,
            decimal? targetDistance = null,
            int? restSeconds = null,
            string? note = null)
            => new(exerciseId, order, targetSets, targetReps, targetWeight, targetDurationSeconds, targetDistance, restSeconds, note);

        internal void SetOrder(int order)
        {
            if (order < 0)
                throw new InvalidArgumentException("Order cannot be negative.");

            Order = order;
        }

        private static void ValidateRange(int? value, int min, int max, string field)
        {
            if (value is int v && (v < min || v > max))
                throw new InvalidArgumentException($"{field} must be between {min} and {max}.");
        }

        private static void ValidateRange(decimal? value, decimal min, decimal max, string field)
        {
            if (value is decimal v && (v < min || v > max))
                throw new InvalidArgumentException($"{field} must be between {min} and {max}.");
        }

        private static void ValidateNote(string? note)
        {
            if (note is not null && note.Trim().Length > NoteMaxLength)
                throw new InvalidArgumentException($"Note cannot exceed {NoteMaxLength} characters.");
        }
    }
}
