using Domain.Calculators;
using Domain.Enums;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Workouts.Calculators
{
    public class OneRepMaxCalculatorTests
    {
        private static readonly DateTime LoggedAt = new(2026, 8, 15, 17, 0, 0, DateTimeKind.Utc);

        private static WorkoutSessionExercise Entry() =>
            WorkoutSessionExercise.CreateFrom(
                Exercise.CreateSystem(1, "Wyciskanie", ExerciseMetricEnum.RepsAndWeight), 0);

        [Fact]
        public void Estimate_ShouldApplyEpley()
        {
            // 100 × (1 + 5/30) = 116.666… → 116.67
            OneRepMaxCalculator.Estimate(5, 100m).Should().Be(116.67m);
        }

        [Fact]
        public void Estimate_ShouldReturnTheWeightItselfForASingle()
        {
            // A 1-rep set *is* the measurement — the formula's 3% inflation would be inventing a number.
            OneRepMaxCalculator.Estimate(1, 140m).Should().Be(140m);
        }

        [Theory]
        [InlineData(null, 100.0)]
        [InlineData(5, null)]
        [InlineData(0, 100.0)]
        [InlineData(5, 0.0)]
        public void Estimate_ShouldReturnNullWithoutAUsableRepsWeightPair(int? reps, double? weight)
        {
            OneRepMaxCalculator.Estimate(reps, (decimal?)weight).Should().BeNull();
        }

        [Fact]
        public void Estimate_ShouldRefuseToGuessFromAVeryHighRepSet()
        {
            OneRepMaxCalculator.Estimate(OneRepMaxCalculator.MaxMeaningfulReps + 1, 40m).Should().BeNull();
        }

        [Fact]
        public void BestEstimate_ShouldTakeTheStrongestWorkingSet()
        {
            var entry = Entry();
            entry.AddSet(LoggedAt, reps: 12, weight: 50m);   // 70
            entry.AddSet(LoggedAt, reps: 5, weight: 80m);    // 93.33
            entry.AddSet(LoggedAt, reps: 8, weight: 70m);    // 88.67

            OneRepMaxCalculator.BestEstimate(entry.Sets).Should().Be(93.33m);
        }

        [Fact]
        public void BestEstimate_ShouldIgnoreWarmups()
        {
            var entry = Entry();
            entry.AddSet(LoggedAt, reps: 1, weight: 200m, setType: WorkoutSetTypeEnum.Warmup);
            entry.AddSet(LoggedAt, reps: 5, weight: 80m);

            OneRepMaxCalculator.BestEstimate(entry.Sets).Should().Be(93.33m);
        }

        [Fact]
        public void BestEstimate_ShouldReturnNullWhenNothingQualifies()
        {
            OneRepMaxCalculator.BestEstimate([]).Should().BeNull();
        }
    }
}
