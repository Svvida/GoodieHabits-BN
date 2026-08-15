using Domain.Calculators;
using FluentAssertions;

namespace Application.Tests.Supplements.Calculators
{
    /// <summary>
    /// The denominator rule is the whole point of this calculator: it is borrowed verbatim from quest
    /// <c>CompletionRate</c>, so that opening the app in the morning never reports the day already missed.
    /// </summary>
    public class SupplementAdherenceCalculatorTests
    {
        private static readonly DateOnly Today = new(2026, 8, 15);

        [Fact]
        public void CountEvaluatedDays_ShouldExcludeTodayWhileItIsStillRunning()
        {
            var evaluated = SupplementAdherenceCalculator.CountEvaluatedDays(
                new DateOnly(2026, 8, 13), new DateOnly(2026, 8, 15), Today, new HashSet<DateOnly>());

            // 13th and 14th have elapsed; today has not.
            evaluated.Should().Be(2);
        }

        [Fact]
        public void CountEvaluatedDays_ShouldCountTodayOnceSomethingWasTaken()
        {
            var evaluated = SupplementAdherenceCalculator.CountEvaluatedDays(
                new DateOnly(2026, 8, 13), new DateOnly(2026, 8, 15), Today, new HashSet<DateOnly> { Today });

            evaluated.Should().Be(3);
        }

        [Fact]
        public void CountEvaluatedDays_ShouldExcludeFutureDays()
        {
            var evaluated = SupplementAdherenceCalculator.CountEvaluatedDays(
                new DateOnly(2026, 8, 16), new DateOnly(2026, 8, 20), Today, new HashSet<DateOnly>());

            evaluated.Should().Be(0);
        }

        [Fact]
        public void CountEvaluatedDays_ShouldReturnZeroForAnInvertedRange()
        {
            var evaluated = SupplementAdherenceCalculator.CountEvaluatedDays(
                new DateOnly(2026, 8, 20), new DateOnly(2026, 8, 10), Today, new HashSet<DateOnly>());

            evaluated.Should().Be(0);
        }

        [Fact]
        public void Calculate_ShouldDivideTakenByScheduled()
        {
            var adherence = SupplementAdherenceCalculator.Calculate(slotsPerDay: 2, evaluatedDays: 10, takenCount: 15);

            adherence.Scheduled.Should().Be(20);
            adherence.Taken.Should().Be(15);
            adherence.Rate.Should().Be(75m);
        }

        [Fact]
        public void Calculate_ShouldReturnANullRateWhenNothingHasBeenEvaluated()
        {
            var adherence = SupplementAdherenceCalculator.Calculate(slotsPerDay: 2, evaluatedDays: 0, takenCount: 0);

            // Null, not zero: "no data" and "you took none of them" must not colour a dashboard the same way.
            adherence.Rate.Should().BeNull();
            adherence.Scheduled.Should().Be(0);
        }

        [Fact]
        public void Calculate_ShouldNotClampAboveOneHundred()
        {
            var adherence = SupplementAdherenceCalculator.Calculate(slotsPerDay: 1, evaluatedDays: 10, takenCount: 12);

            // Extra doses are something the user really did; hiding it would make the number a judgement.
            adherence.Rate.Should().Be(120m);
        }

        [Fact]
        public void Calculate_ShouldRejectNegativeInputs()
        {
            var act = () => SupplementAdherenceCalculator.Calculate(1, 10, -1);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}
