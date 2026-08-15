using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Models
{
    /// <summary>
    /// One exercise as it appeared in a performed session, together with the sets logged against it.
    /// <para>
    /// It snapshots <see cref="ExerciseName"/> <em>and</em> <see cref="MetricType"/> alongside a nullable
    /// <see cref="ExerciseId"/>. Without the snapshot, renaming "Wyciskanie" to "Wyciskanie sztangi" silently
    /// rewrites a year of history, and changing an exercise's metric makes old rows unrenderable. The FK is
    /// <c>SetNull</c>, so history survives even a deleted exercise.
    /// </para>
    /// </summary>
    public class WorkoutSessionExercise : EntityBase
    {
        public const int NoteMaxLength = 250;

        public int Id { get; set; }
        public int WorkoutSessionId { get; private set; }

        /// <summary>The library exercise, or null once that exercise has been deleted. History reads the snapshot.</summary>
        public int? ExerciseId { get; private set; }

        public string ExerciseName { get; private set; } = null!;
        public ExerciseMetricEnum MetricType { get; private set; }
        public int Order { get; private set; }

        public int? TargetSets { get; private set; }
        public int? TargetReps { get; private set; }
        public decimal? TargetWeight { get; private set; }
        public int? TargetDurationSeconds { get; private set; }
        public decimal? TargetDistance { get; private set; }
        public int? RestSeconds { get; private set; }
        public string? Note { get; private set; }

        public WorkoutSession WorkoutSession { get; set; } = null!;
        public Exercise? Exercise { get; set; }
        public ICollection<WorkoutSet> Sets { get; set; } = [];

        protected WorkoutSessionExercise() { }

        private WorkoutSessionExercise(
            int? exerciseId,
            string exerciseName,
            ExerciseMetricEnum metricType,
            int order,
            int? targetSets,
            int? targetReps,
            decimal? targetWeight,
            int? targetDurationSeconds,
            decimal? targetDistance,
            int? restSeconds,
            string? note)
        {
            if (string.IsNullOrWhiteSpace(exerciseName))
                throw new InvalidArgumentException("Exercise name cannot be null or whitespace.");
            if (order < 0)
                throw new InvalidArgumentException("Order cannot be negative.");
            if (note is not null && note.Trim().Length > NoteMaxLength)
                throw new InvalidArgumentException($"Note cannot exceed {NoteMaxLength} characters.");

            ExerciseId = exerciseId;
            ExerciseName = exerciseName.Trim();
            MetricType = metricType;
            Order = order;
            TargetSets = targetSets;
            TargetReps = targetReps;
            TargetWeight = targetWeight;
            TargetDurationSeconds = targetDurationSeconds;
            TargetDistance = targetDistance;
            RestSeconds = restSeconds;
            Note = note?.Trim();
        }

        /// <summary>Adds a library exercise to a session, snapshotting the fields history depends on.</summary>
        public static WorkoutSessionExercise CreateFrom(
            Exercise exercise,
            int order,
            int? targetSets = null,
            int? targetReps = null,
            decimal? targetWeight = null,
            int? targetDurationSeconds = null,
            decimal? targetDistance = null,
            int? restSeconds = null,
            string? note = null)
        {
            ArgumentNullException.ThrowIfNull(exercise);

            return new WorkoutSessionExercise(
                exercise.Id,
                exercise.Name,
                exercise.MetricType,
                order,
                targetSets,
                targetReps,
                targetWeight,
                targetDurationSeconds,
                targetDistance,
                restSeconds,
                note);
        }

        /// <summary>
        /// Materializes a routine item into a session. <paramref name="item"/> must have its
        /// <c>Exercise</c> loaded — the snapshot cannot be taken from a partially loaded graph.
        /// </summary>
        public static WorkoutSessionExercise CreateFromTemplate(WorkoutRoutineExercise item, int order)
        {
            ArgumentNullException.ThrowIfNull(item);

            if (item.Exercise is null)
                throw new InvalidArgumentException("Routine exercise must be loaded with its Exercise.");

            return CreateFrom(
                item.Exercise,
                order,
                item.TargetSets,
                item.TargetReps,
                item.TargetWeight,
                item.TargetDurationSeconds,
                item.TargetDistance,
                item.RestSeconds,
                item.Note);
        }

        /// <summary>
        /// Logs a set. The set is validated against this entry's snapshotted <see cref="MetricType"/>, which is
        /// why sets are created through the parent rather than standalone: the metric is the invariant.
        /// </summary>
        public WorkoutSet AddSet(
            DateTime completedAtUtc,
            int? reps = null,
            decimal? weight = null,
            int? durationSeconds = null,
            decimal? distance = null,
            decimal? rpe = null,
            WorkoutSetTypeEnum setType = WorkoutSetTypeEnum.Normal)
        {
            var set = WorkoutSet.Create(
                MetricType, NextSetNumber(), completedAtUtc, reps, weight, durationSeconds, distance, rpe, setType);

            Sets.Add(set);
            return set;
        }

        public void RemoveSet(WorkoutSet set)
        {
            ArgumentNullException.ThrowIfNull(set);

            if (!Sets.Remove(set))
                throw new NotFoundException("Set does not belong to this exercise entry.");

            RenumberSets();
        }

        /// <summary>Replaces every logged set, renumbering from 1. Used by the bulk logging path.</summary>
        public void ReplaceSets(IEnumerable<WorkoutSet> sets)
        {
            ArgumentNullException.ThrowIfNull(sets);

            var ordered = sets.ToList();

            Sets.Clear();

            for (var i = 0; i < ordered.Count; i++)
            {
                ordered[i].SetNumberTo(i + 1);
                Sets.Add(ordered[i]);
            }
        }

        public void UpdateTargets(
            int? targetSets,
            int? targetReps,
            decimal? targetWeight,
            int? targetDurationSeconds,
            decimal? targetDistance,
            int? restSeconds)
        {
            TargetSets = targetSets;
            TargetReps = targetReps;
            TargetWeight = targetWeight;
            TargetDurationSeconds = targetDurationSeconds;
            TargetDistance = targetDistance;
            RestSeconds = restSeconds;
        }

        public void UpdateNote(string? note)
        {
            if (note is not null && note.Trim().Length > NoteMaxLength)
                throw new InvalidArgumentException($"Note cannot exceed {NoteMaxLength} characters.");

            Note = note?.Trim();
        }

        /// <summary>Clears the library link so an exercise can be deleted without taking history with it.</summary>
        public void DetachExercise() => ExerciseId = null;

        internal void SetOrder(int order)
        {
            if (order < 0)
                throw new InvalidArgumentException("Order cannot be negative.");

            Order = order;
        }

        private int NextSetNumber() => Sets.Count == 0 ? 1 : Sets.Max(s => s.SetNumber) + 1;

        private void RenumberSets()
        {
            var i = 1;
            foreach (var set in Sets.OrderBy(s => s.SetNumber).ToList())
                set.SetNumberTo(i++);
        }
    }
}
