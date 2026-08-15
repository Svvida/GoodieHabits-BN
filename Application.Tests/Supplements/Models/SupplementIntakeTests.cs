using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Supplements.Models
{
    public class SupplementIntakeTests
    {
        private static readonly DateOnly Today = new(2026, 8, 15);
        private static readonly DateTime TakenAt = new(2026, 8, 15, 7, 30, 0, DateTimeKind.Utc);

        private static Supplement Magnesium(int id = 1, decimal? defaultAmount = 1m)
        {
            var supplement = Supplement.Create(1, "Magnez", SupplementUnitEnum.Capsule, defaultAmount);
            supplement.Id = id;
            return supplement;
        }

        [Fact]
        public void Create_ShouldPreferAnExplicitAmount()
        {
            var supplement = Magnesium();
            var slot = supplement.AddSlot(SupplementTimingEnum.Morning, 2m);

            var intake = SupplementIntake.Create(supplement, Today, TakenAt, slot, amount: 3m);

            intake.Amount.Should().Be(3m);
        }

        [Fact]
        public void Create_ShouldFallBackToTheSlotAmount()
        {
            var supplement = Magnesium();
            var slot = supplement.AddSlot(SupplementTimingEnum.Morning, 2m);

            var intake = SupplementIntake.Create(supplement, Today, TakenAt, slot);

            // Ticking a checkbox needs no payload beyond the intent.
            intake.Amount.Should().Be(2m);
            intake.ScheduleSlotId.Should().Be(slot.Id);
            intake.IsAdHoc.Should().BeFalse();
        }

        [Fact]
        public void Create_ShouldFallBackToTheSupplementDefaultForAnAdHocDose()
        {
            var supplement = Magnesium();

            var intake = SupplementIntake.Create(supplement, Today, TakenAt);

            intake.Amount.Should().Be(1m);
            intake.IsAdHoc.Should().BeTrue();
        }

        [Fact]
        public void Create_ShouldRejectADoseWithNoAmountAnywhere()
        {
            var supplement = Magnesium(defaultAmount: null);

            var act = () => SupplementIntake.Create(supplement, Today, TakenAt);

            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void Create_ShouldRejectASlotBelongingToAnotherSupplement()
        {
            var magnesium = Magnesium(id: 1);
            var creatine = Supplement.Create(1, "Kreatyna", SupplementUnitEnum.Gram);
            creatine.Id = 2;
            var foreignSlot = creatine.AddSlot(SupplementTimingEnum.PreWorkout, 5m);

            var act = () => SupplementIntake.Create(magnesium, Today, TakenAt, foreignSlot);

            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void Create_ShouldCarryTheOwnerFromTheSupplement()
        {
            var intake = SupplementIntake.Create(Magnesium(), Today, TakenAt);

            intake.UserProfileId.Should().Be(1);
            intake.SupplementId.Should().Be(1);
        }

        [Fact]
        public void Create_ShouldOptionallyLinkTheDoseToATrainingSession()
        {
            var supplement = Magnesium();
            var slot = supplement.AddSlot(SupplementTimingEnum.PreWorkout, 1m, offsetMinutes: -30);

            var intake = SupplementIntake.Create(supplement, Today, TakenAt, slot, workoutSessionId: 99);

            // The only coupling between the supplements and workouts modules.
            intake.WorkoutSessionId.Should().Be(99);
        }

        [Fact]
        public void DetachFromSlot_ShouldKeepTheDoseAsAnAdHocRecord()
        {
            var supplement = Magnesium();
            var slot = supplement.AddSlot(SupplementTimingEnum.Morning, 2m);
            var intake = SupplementIntake.Create(supplement, Today, TakenAt, slot);

            intake.DetachFromSlot();

            intake.ScheduleSlotId.Should().BeNull();
            intake.IsAdHoc.Should().BeTrue();
            intake.Amount.Should().Be(2m);
        }

        [Fact]
        public void UpdateAmount_ShouldRejectANonPositiveValue()
        {
            var intake = SupplementIntake.Create(Magnesium(), Today, TakenAt);

            var act = () => intake.UpdateAmount(0m);

            act.Should().Throw<InvalidArgumentException>();
        }
    }
}
