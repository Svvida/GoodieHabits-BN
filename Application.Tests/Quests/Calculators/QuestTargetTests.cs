using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;
using FluentAssertions;

namespace Application.Tests.Quests.Calculators
{
    public class QuestTargetTests
    {
        [Fact]
        public void Once_ShouldReproduceTheOldBooleanBehaviour()
        {
            var target = QuestTarget.Once();

            target.Amount.Should().Be(1m);
            target.IsSingleTick.Should().BeTrue();
        }

        [Fact]
        public void AFractionalAmountCountedInTimes_ShouldBeRejected()
        {
            var act = () => QuestTarget.Create(2.5m);

            act.Should().Throw<InvalidArgumentException>()
                .WithMessage("*whole number*");
        }

        [Fact]
        public void AFractionalAmountWithAUnit_ShouldBeAllowed()
        {
            var target = QuestTarget.Create(2.5m, "L");

            target.Amount.Should().Be(2.5m);
            target.Unit.Should().Be("L");
            target.IsSingleTick.Should().BeFalse();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ANonPositiveAmount_ShouldBeRejected(int amount)
        {
            var act = () => QuestTarget.Create(amount);

            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void MaxCompletionsPerDayBelowOne_ShouldBeRejected()
        {
            var act = () => QuestTarget.Create(2, maxCompletionsPerDay: 0);

            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void ProratedAmount_ShouldRoundUpSoAPartialPeriodStillAsksForSomething()
        {
            var target = QuestTarget.Create(3, maxCompletionsPerDay: 1);

            // A "3 times a week" quest created on a Saturday: two of seven days left.
            target.ProratedAmount(activeDays: 2, fullDays: 7).Should().Be(1m);
        }

        [Fact]
        public void ProratedAmount_ShouldNeverExceedTheFullTarget()
        {
            var target = QuestTarget.Create(3);

            target.ProratedAmount(activeDays: 7, fullDays: 7).Should().Be(3m);
            target.ProratedAmount(activeDays: 9, fullDays: 7).Should().Be(3m);
        }

        [Fact]
        public void ProratedAmount_ShouldKeepFractionsForAMeasuredTarget()
        {
            var target = QuestTarget.Create(2m, "L");

            target.ProratedAmount(activeDays: 1, fullDays: 2).Should().Be(1m);
        }

        [Fact]
        public void AtMost_ShouldBeRepresentableEvenThoughTheBehaviourShipsLater()
        {
            var target = QuestTarget.Create(2, mode: TargetModeEnum.AtMost);

            target.Mode.Should().Be(TargetModeEnum.AtMost);
        }
    }
}
