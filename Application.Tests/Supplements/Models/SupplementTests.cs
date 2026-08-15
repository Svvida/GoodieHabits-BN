using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Supplements.Models
{
    public class SupplementTests
    {
        private static Supplement Magnesium(int id = 0)
        {
            var supplement = Supplement.Create(1, "Magnez", SupplementUnitEnum.Capsule, defaultAmount: 1m);
            supplement.Id = id;
            return supplement;
        }

        [Fact]
        public void Create_ShouldTrimTheNameAndStartActive()
        {
            var supplement = Supplement.Create(1, "  Kreatyna  ", SupplementUnitEnum.Gram, defaultAmount: 5m);

            supplement.Name.Should().Be("Kreatyna");
            supplement.IsActive.Should().BeTrue();
            supplement.Unit.Should().Be(SupplementUnitEnum.Gram);
        }

        [Fact]
        public void Create_ShouldRejectANonPositiveDefaultAmount()
        {
            var act = () => Supplement.Create(1, "Kreatyna", SupplementUnitEnum.Gram, defaultAmount: 0m);

            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void Create_ShouldRejectAMalformedColor()
        {
            var act = () => Supplement.Create(1, "Kreatyna", SupplementUnitEnum.Gram, color: "red");

            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void AddSlot_ShouldLetOneSupplementCarrySeveralDosesADay()
        {
            var supplement = Magnesium();

            supplement.AddSlot(SupplementTimingEnum.Morning, 1m);
            supplement.AddSlot(SupplementTimingEnum.Night, 2m);

            // Magnesium morning *and* evening is one supplement with two slots, not two supplements.
            supplement.Slots.Should().HaveCount(2);
            supplement.Slots.Select(s => s.Amount).Should().Equal(1m, 2m);
        }

        [Fact]
        public void AddSlot_ShouldStampTheOwningSupplement()
        {
            var supplement = Magnesium(id: 7);

            var slot = supplement.AddSlot(SupplementTimingEnum.Morning, 1m);

            slot.SupplementId.Should().Be(7);
        }

        [Fact]
        public void RemoveSlot_ShouldRejectASlotFromAnotherSupplement()
        {
            var supplement = Magnesium();
            var foreign = Supplement.Create(1, "Kreatyna", SupplementUnitEnum.Gram).AddSlot(SupplementTimingEnum.PreWorkout, 5m);

            var act = () => supplement.RemoveSlot(foreign);

            act.Should().Throw<NotFoundException>();
        }

        [Fact]
        public void SetActive_ShouldBeTheRetirePathThatKeepsHistory()
        {
            var supplement = Magnesium();
            supplement.AddSlot(SupplementTimingEnum.Morning, 1m);

            supplement.SetActive(false);

            supplement.IsActive.Should().BeFalse();
            supplement.Slots.Should().ContainSingle();
        }

        [Fact]
        public void Slot_ShouldRequireATimeOfDayForACustomTiming()
        {
            var supplement = Magnesium();

            var act = () => supplement.AddSlot(SupplementTimingEnum.Custom, 1m);

            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void Slot_ShouldCarryAWorkoutRelativeOffset()
        {
            var supplement = Supplement.Create(1, "Kreatyna", SupplementUnitEnum.Gram);

            // "30 min przed treningiem"
            var slot = supplement.AddSlot(SupplementTimingEnum.PreWorkout, 5m, offsetMinutes: -30);

            slot.Timing.Should().Be(SupplementTimingEnum.PreWorkout);
            slot.OffsetMinutes.Should().Be(-30);
        }

        [Fact]
        public void Slot_ShouldRejectAnOffsetOutsideItsBounds()
        {
            var supplement = Magnesium();

            var act = () => supplement.AddSlot(SupplementTimingEnum.PreWorkout, 1m, offsetMinutes: -5000);

            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void Slot_ShouldRejectANonPositiveAmount()
        {
            var supplement = Magnesium();

            var act = () => supplement.AddSlot(SupplementTimingEnum.Morning, 0m);

            act.Should().Throw<InvalidArgumentException>();
        }
    }
}
