using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Workouts.Models
{
    /// <summary>
    /// The metric contract: which measurements a set must carry, and — just as deliberately — which extra ones
    /// it is allowed to carry anyway.
    /// </summary>
    public class WorkoutSetTests
    {
        private static readonly DateTime LoggedAt = new(2026, 8, 15, 17, 45, 0, DateTimeKind.Utc);

        private static WorkoutSessionExercise Entry(ExerciseMetricEnum metric) =>
            WorkoutSessionExercise.CreateFrom(Exercise.CreateSystem(1, "Ćwiczenie", metric), 0);

        [Theory]
        [InlineData(ExerciseMetricEnum.Reps)]
        [InlineData(ExerciseMetricEnum.RepsAndWeight)]
        [InlineData(ExerciseMetricEnum.Time)]
        [InlineData(ExerciseMetricEnum.Distance)]
        [InlineData(ExerciseMetricEnum.DistanceAndTime)]
        public void AddSet_ShouldRejectASetMissingWhatItsMetricRequires(ExerciseMetricEnum metric)
        {
            var entry = Entry(metric);

            var act = () => entry.AddSet(LoggedAt);

            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void AddSet_ShouldAcceptRepsOnlyForARepsExercise()
        {
            var set = Entry(ExerciseMetricEnum.Reps).AddSet(LoggedAt, reps: 12);

            set.Reps.Should().Be(12);
            set.Weight.Should().BeNull();
        }

        [Fact]
        public void AddSet_ShouldRequireBothMeasurementsForDistanceAndTime()
        {
            var entry = Entry(ExerciseMetricEnum.DistanceAndTime);

            var act = () => entry.AddSet(LoggedAt, distance: 5000m);

            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void AddSet_ShouldAllowAMeasurementTheMetricDoesNotRequire()
        {
            // The metric governs what is *required*, never what is forbidden — this is how a weighted pull-up
            // gets recorded against a Reps exercise.
            var set = Entry(ExerciseMetricEnum.Reps).AddSet(LoggedAt, reps: 8, weight: 20m);

            set.Reps.Should().Be(8);
            set.Weight.Should().Be(20m);
        }

        [Fact]
        public void AddSet_ShouldNumberSetsFromOne()
        {
            var entry = Entry(ExerciseMetricEnum.RepsAndWeight);

            entry.AddSet(LoggedAt, reps: 8, weight: 60m);
            entry.AddSet(LoggedAt, reps: 8, weight: 60m);
            entry.AddSet(LoggedAt, reps: 6, weight: 65m);

            entry.Sets.Select(s => s.SetNumber).Should().Equal(1, 2, 3);
        }

        [Fact]
        public void RemoveSet_ShouldRenumberWhatIsLeft()
        {
            var entry = Entry(ExerciseMetricEnum.RepsAndWeight);
            var first = entry.AddSet(LoggedAt, reps: 8, weight: 60m);
            entry.AddSet(LoggedAt, reps: 8, weight: 60m);
            entry.AddSet(LoggedAt, reps: 6, weight: 65m);

            entry.RemoveSet(first);

            entry.Sets.Select(s => s.SetNumber).Should().Equal(1, 2);
        }

        [Fact]
        public void RemoveSet_ShouldRejectASetFromAnotherEntry()
        {
            var entry = Entry(ExerciseMetricEnum.RepsAndWeight);
            var foreign = Entry(ExerciseMetricEnum.RepsAndWeight).AddSet(LoggedAt, reps: 8, weight: 60m);

            var act = () => entry.RemoveSet(foreign);

            act.Should().Throw<NotFoundException>();
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(10.5)]
        public void AddSet_ShouldRejectAnRpeOutsideOneToTen(double rpe)
        {
            var entry = Entry(ExerciseMetricEnum.Reps);

            var act = () => entry.AddSet(LoggedAt, reps: 8, rpe: (decimal)rpe);

            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void AddSet_ShouldRejectRepsBeyondTheLimit()
        {
            var entry = Entry(ExerciseMetricEnum.Reps);

            var act = () => entry.AddSet(LoggedAt, reps: 100_000);

            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void WarmupSets_ShouldBeMarkedAsSuch()
        {
            var entry = Entry(ExerciseMetricEnum.RepsAndWeight);

            var warmup = entry.AddSet(LoggedAt, reps: 10, weight: 20m, setType: WorkoutSetTypeEnum.Warmup);
            var working = entry.AddSet(LoggedAt, reps: 8, weight: 60m);

            warmup.IsWarmup.Should().BeTrue();
            working.IsWarmup.Should().BeFalse();
        }

        [Fact]
        public void ReplaceSets_ShouldRenumberFromOne()
        {
            var entry = Entry(ExerciseMetricEnum.RepsAndWeight);

            entry.ReplaceSets(
            [
                WorkoutSet.Create(ExerciseMetricEnum.RepsAndWeight, 9, LoggedAt, reps: 8, weight: 60m),
                WorkoutSet.Create(ExerciseMetricEnum.RepsAndWeight, 3, LoggedAt, reps: 6, weight: 65m),
            ]);

            entry.Sets.Select(s => s.SetNumber).Should().Equal(1, 2);
        }
    }
}
