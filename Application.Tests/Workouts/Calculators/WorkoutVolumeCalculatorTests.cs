using Domain.Calculators;
using Domain.Enums;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Workouts.Calculators
{
    /// <summary>
    /// Phase 1. Pure aggregation — deliberately not exercised through the DB, because the InMemory provider
    /// would happily pass a sum that real SQL gets wrong (ARCHITECTURE §10).
    /// </summary>
    public class WorkoutVolumeCalculatorTests
    {
        private static readonly DateOnly Today = new(2026, 8, 15);
        private static readonly DateTime StartedAt = new(2026, 8, 15, 17, 0, 0, DateTimeKind.Utc);

        private static WorkoutSessionExercise Entry(ExerciseMetricEnum metric = ExerciseMetricEnum.RepsAndWeight) =>
            WorkoutSessionExercise.CreateFrom(Exercise.CreateSystem(1, "Wyciskanie", metric), 0);

        [Fact]
        public void CalculateVolume_ShouldMultiplyRepsByWeight()
        {
            var entry = Entry();
            entry.AddSet(StartedAt, reps: 8, weight: 60m);
            entry.AddSet(StartedAt, reps: 6, weight: 65m);

            WorkoutVolumeCalculator.CalculateVolume(entry.Sets).Should().Be(8 * 60m + 6 * 65m);
        }

        [Fact]
        public void CalculateVolume_ShouldExcludeWarmupsByDefault()
        {
            var entry = Entry();
            entry.AddSet(StartedAt, reps: 10, weight: 20m, setType: WorkoutSetTypeEnum.Warmup);
            entry.AddSet(StartedAt, reps: 8, weight: 60m);

            WorkoutVolumeCalculator.CalculateVolume(entry.Sets).Should().Be(480m);
            WorkoutVolumeCalculator.CalculateVolume(entry.Sets, includeWarmups: true).Should().Be(680m);
        }

        [Fact]
        public void CalculateVolume_ShouldTreatAWeightlessSetAsZeroVolume()
        {
            var entry = Entry(ExerciseMetricEnum.Reps);
            entry.AddSet(StartedAt, reps: 12);

            // Bodyweight work has no volume until bodyweight tracking exists; the reps still count elsewhere.
            WorkoutVolumeCalculator.CalculateVolume(entry.Sets).Should().Be(0m);
        }

        [Fact]
        public void CalculateVolume_ShouldReturnZeroForNoSets()
        {
            WorkoutVolumeCalculator.CalculateVolume([]).Should().Be(0m);
        }

        [Fact]
        public void CalculateSessionTotals_ShouldRollUpTheWholeSession()
        {
            var session = WorkoutSession.Start(1, "Push A", Today, StartedAt);

            var bench = session.AddExercise(
                Exercise.CreateSystem(1, "Wyciskanie", ExerciseMetricEnum.RepsAndWeight));
            bench.AddSet(StartedAt, reps: 10, weight: 20m, setType: WorkoutSetTypeEnum.Warmup);
            bench.AddSet(StartedAt, reps: 8, weight: 60m);
            bench.AddSet(StartedAt, reps: 8, weight: 60m);

            var pullUps = session.AddExercise(
                Exercise.CreateSystem(2, "Podciąganie", ExerciseMetricEnum.Reps));
            pullUps.AddSet(StartedAt, reps: 6);

            session.Complete(StartedAt.AddMinutes(50));

            var totals = WorkoutVolumeCalculator.CalculateSessionTotals(session);

            totals.ExerciseCount.Should().Be(2);
            totals.SetCount.Should().Be(3);           // the warm-up is excluded
            totals.TotalReps.Should().Be(8 + 8 + 6);
            totals.TotalVolume.Should().Be(960m);
            totals.DurationSeconds.Should().Be(50 * 60);
        }

        [Fact]
        public void CalculateSessionTotals_ShouldLeaveDurationNullWhileTheSessionIsRunning()
        {
            var session = WorkoutSession.Start(1, "Push A", Today, StartedAt);

            WorkoutVolumeCalculator.CalculateSessionTotals(session).DurationSeconds.Should().BeNull();
        }
    }
}
