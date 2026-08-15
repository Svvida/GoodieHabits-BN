using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Models
{
    /// <summary>
    /// One logged set. All four measurement columns are nullable; which of them are <em>required</em> is
    /// decided by the parent entry's <see cref="ExerciseMetricEnum"/>.
    /// <para>
    /// The metric governs what is required, never what is forbidden: logging weight on a
    /// <see cref="ExerciseMetricEnum.Reps"/> exercise is how a weighted pull-up gets recorded, and rejecting it
    /// would be restriction for its own sake. Extra measurements are stored and returned as given.
    /// </para>
    /// </summary>
    public class WorkoutSet : EntityBase
    {
        public int Id { get; set; }
        public int WorkoutSessionExerciseId { get; private set; }

        /// <summary>Position within the exercise entry, 1-based and contiguous.</summary>
        public int SetNumber { get; private set; }

        public int? Reps { get; private set; }
        public decimal? Weight { get; private set; }
        public int? DurationSeconds { get; private set; }
        public decimal? Distance { get; private set; }

        /// <summary>Rate of perceived exertion, 1-10 in half steps. Optional and purely descriptive.</summary>
        public decimal? Rpe { get; private set; }

        public WorkoutSetTypeEnum SetType { get; private set; }

        /// <summary>A genuine instant — when the set was logged. UTC, unlike the session's calendar date.</summary>
        public DateTime CompletedAt { get; private set; }

        public WorkoutSessionExercise WorkoutSessionExercise { get; set; } = null!;

        /// <summary>Warm-ups are excluded from volume and personal records by default.</summary>
        public bool IsWarmup => SetType == WorkoutSetTypeEnum.Warmup;

        protected WorkoutSet() { }

        private WorkoutSet(
            int setNumber,
            DateTime completedAtUtc,
            int? reps,
            decimal? weight,
            int? durationSeconds,
            decimal? distance,
            decimal? rpe,
            WorkoutSetTypeEnum setType)
        {
            if (setNumber <= 0)
                throw new InvalidArgumentException("SetNumber must be greater than zero.");

            SetNumber = setNumber;
            CompletedAt = completedAtUtc;
            Reps = reps;
            Weight = weight;
            DurationSeconds = durationSeconds;
            Distance = distance;
            Rpe = rpe;
            SetType = setType;
        }

        public static WorkoutSet Create(
            ExerciseMetricEnum metricType,
            int setNumber,
            DateTime completedAtUtc,
            int? reps = null,
            decimal? weight = null,
            int? durationSeconds = null,
            decimal? distance = null,
            decimal? rpe = null,
            WorkoutSetTypeEnum setType = WorkoutSetTypeEnum.Normal)
        {
            ValidateForMetric(metricType, reps, weight, durationSeconds, distance);
            ValidateBounds(reps, weight, durationSeconds, distance, rpe);

            return new WorkoutSet(setNumber, completedAtUtc, reps, weight, durationSeconds, distance, rpe, setType);
        }

        public void Update(
            ExerciseMetricEnum metricType,
            int? reps,
            decimal? weight,
            int? durationSeconds,
            decimal? distance,
            decimal? rpe,
            WorkoutSetTypeEnum setType)
        {
            ValidateForMetric(metricType, reps, weight, durationSeconds, distance);
            ValidateBounds(reps, weight, durationSeconds, distance, rpe);

            Reps = reps;
            Weight = weight;
            DurationSeconds = durationSeconds;
            Distance = distance;
            Rpe = rpe;
            SetType = setType;
        }

        internal void SetNumberTo(int setNumber)
        {
            if (setNumber <= 0)
                throw new InvalidArgumentException("SetNumber must be greater than zero.");

            SetNumber = setNumber;
        }

        /// <summary>
        /// Which measurements a metric requires. Public so the FluentValidation rules for the logging
        /// endpoints can reuse the single source of truth instead of restating the table.
        /// </summary>
        public static void ValidateForMetric(
            ExerciseMetricEnum metricType,
            int? reps,
            decimal? weight,
            int? durationSeconds,
            decimal? distance)
        {
            switch (metricType)
            {
                case ExerciseMetricEnum.Reps:
                    Require(reps, nameof(Reps), metricType);
                    break;

                case ExerciseMetricEnum.RepsAndWeight:
                    Require(reps, nameof(Reps), metricType);
                    Require(weight, nameof(Weight), metricType);
                    break;

                case ExerciseMetricEnum.Time:
                    Require(durationSeconds, nameof(DurationSeconds), metricType);
                    break;

                case ExerciseMetricEnum.Distance:
                    Require(distance, nameof(Distance), metricType);
                    break;

                case ExerciseMetricEnum.DistanceAndTime:
                    Require(distance, nameof(Distance), metricType);
                    Require(durationSeconds, nameof(DurationSeconds), metricType);
                    break;

                default:
                    throw new InvalidArgumentException($"Unsupported exercise metric '{metricType}'.");
            }
        }

        private static void ValidateBounds(
            int? reps, decimal? weight, int? durationSeconds, decimal? distance, decimal? rpe)
        {
            if (reps is int r && (r < 0 || r > WorkoutLimits.MaxReps))
                throw new InvalidArgumentException($"Reps must be between 0 and {WorkoutLimits.MaxReps}.");

            if (weight is decimal w && (w < 0 || w > WorkoutLimits.MaxWeight))
                throw new InvalidArgumentException($"Weight must be between 0 and {WorkoutLimits.MaxWeight}.");

            if (durationSeconds is int d && (d < 0 || d > WorkoutLimits.MaxDurationSeconds))
                throw new InvalidArgumentException($"DurationSeconds must be between 0 and {WorkoutLimits.MaxDurationSeconds}.");

            if (distance is decimal dist && (dist < 0 || dist > WorkoutLimits.MaxDistance))
                throw new InvalidArgumentException($"Distance must be between 0 and {WorkoutLimits.MaxDistance}.");

            if (rpe is decimal value && (value < WorkoutLimits.MinRpe || value > WorkoutLimits.MaxRpe))
                throw new InvalidArgumentException($"Rpe must be between {WorkoutLimits.MinRpe} and {WorkoutLimits.MaxRpe}.");
        }

        private static void Require(int? value, string field, ExerciseMetricEnum metricType)
        {
            if (value is null)
                throw new InvalidArgumentException($"{field} is required for a '{metricType}' exercise.");
        }

        private static void Require(decimal? value, string field, ExerciseMetricEnum metricType)
        {
            if (value is null)
                throw new InvalidArgumentException($"{field} is required for a '{metricType}' exercise.");
        }
    }
}
