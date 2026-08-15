using Application.Supplements.Analytics.Queries.GetAdherence;
using Domain.Enums;
using Domain.Models;
using FluentAssertions;
using Moq;
using NodaTime;

namespace Application.Tests.Supplements.Analytics
{
    /// <summary>
    /// The denominator rule is the whole point: today must not count against the user while it is still
    /// running. The clock is pinned to 2026-08-15 12:00 UTC for these tests.
    /// </summary>
    public class GetAdherenceQueryHandlerTests : TestBase<GetAdherenceQueryHandler>
    {
        private static readonly DateOnly Today = new(2026, 8, 15);
        private static readonly Instant Noon = Instant.FromUtc(2026, 8, 15, 12, 0, 0);

        private readonly GetAdherenceQueryHandler _handler;

        public GetAdherenceQueryHandlerTests()
        {
            _clockMock.Setup(c => c.GetCurrentInstant()).Returns(Noon);
            _handler = new GetAdherenceQueryHandler(_unitOfWork, _clockMock.Object);
        }

        private async Task<UserProfile> CreateProfileAsync()
        {
            var account = await AddAccountAsync("user@test.com", "pass", "user");
            return account.Profile;
        }

        private async Task<Supplement> AddSupplementAsync(int userProfileId, string name, int slotCount)
        {
            var supplement = Supplement.Create(userProfileId, name, SupplementUnitEnum.Capsule, 1m);
            _context.Supplements.Add(supplement);
            await _context.SaveChangesAsync();

            for (var i = 0; i < slotCount; i++)
                supplement.AddSlot(i == 0 ? SupplementTimingEnum.Morning : SupplementTimingEnum.Night, 1m);

            await _context.SaveChangesAsync();
            return supplement;
        }

        private async Task TickAsync(Supplement supplement, SupplementScheduleSlot slot, DateOnly day)
        {
            _context.SupplementIntakes.Add(
                SupplementIntake.Create(supplement, day, Noon.ToDateTimeUtc(), slot));
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task Handle_ShouldNotCountTodayWhileItIsStillRunning()
        {
            var profile = await CreateProfileAsync();
            var supplement = await AddSupplementAsync(profile.Id, "Magnez", slotCount: 1);
            var slot = supplement.Slots.Single();

            await TickAsync(supplement, slot, Today.AddDays(-2));
            await TickAsync(supplement, slot, Today.AddDays(-1));

            var result = await _handler.Handle(
                new GetAdherenceQuery(profile.Id, Today.AddDays(-2), Today), CancellationToken.None);

            // The 13th and 14th have elapsed; the 15th has not and nothing was taken on it.
            result.Scheduled.Should().Be(2);
            result.Taken.Should().Be(2);
            result.Rate.Should().Be(100m);
        }

        [Fact]
        public async Task Handle_ShouldCountTodayOnceSomethingWasTaken()
        {
            var profile = await CreateProfileAsync();
            var supplement = await AddSupplementAsync(profile.Id, "Magnez", slotCount: 1);
            var slot = supplement.Slots.Single();

            await TickAsync(supplement, slot, Today.AddDays(-1));
            await TickAsync(supplement, slot, Today);

            var result = await _handler.Handle(
                new GetAdherenceQuery(profile.Id, Today.AddDays(-1), Today), CancellationToken.None);

            result.Scheduled.Should().Be(2);
            result.Taken.Should().Be(2);
        }

        [Fact]
        public async Task Handle_ShouldMultiplyTheDenominatorByTheSlotsPerDay()
        {
            var profile = await CreateProfileAsync();
            var supplement = await AddSupplementAsync(profile.Id, "Magnez", slotCount: 2);
            var morning = supplement.Slots.First();

            await TickAsync(supplement, morning, Today.AddDays(-2));
            await TickAsync(supplement, morning, Today.AddDays(-1));

            var result = await _handler.Handle(
                new GetAdherenceQuery(profile.Id, Today.AddDays(-2), Today.AddDays(-1)), CancellationToken.None);

            var item = result.Items.Single();
            item.SlotsPerDay.Should().Be(2);
            item.Scheduled.Should().Be(4);   // 2 slots × 2 elapsed days
            item.Taken.Should().Be(2);
            item.Rate.Should().Be(50m);
        }

        [Fact]
        public async Task Handle_ShouldReturnANullRateWhenNothingHasBeenEvaluated()
        {
            var profile = await CreateProfileAsync();
            await AddSupplementAsync(profile.Id, "Magnez", slotCount: 1);

            var result = await _handler.Handle(
                new GetAdherenceQuery(profile.Id, Today, Today.AddDays(3)), CancellationToken.None);

            // Null, not 0% — "no data" and "you took none of them" must not colour a dashboard the same way.
            result.Rate.Should().BeNull();
            result.Items.Single().Rate.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ShouldNotCountAdHocDosesTowardThePlan()
        {
            var profile = await CreateProfileAsync();
            var supplement = await AddSupplementAsync(profile.Id, "Magnez", slotCount: 1);

            _context.SupplementIntakes.Add(
                SupplementIntake.Create(supplement, Today.AddDays(-1), Noon.ToDateTimeUtc()));
            await _context.SaveChangesAsync();

            var result = await _handler.Handle(
                new GetAdherenceQuery(profile.Id, Today.AddDays(-1), Today.AddDays(-1)), CancellationToken.None);

            // An unplanned dose must not paper over a plan that isn't being followed.
            result.Scheduled.Should().Be(1);
            result.Taken.Should().Be(0);
            result.Rate.Should().Be(0m);
        }

        [Fact]
        public async Task Handle_ShouldSkipSupplementsWithNoSchedule()
        {
            var profile = await CreateProfileAsync();
            await AddSupplementAsync(profile.Id, "Bez planu", slotCount: 0);

            var result = await _handler.Handle(
                new GetAdherenceQuery(profile.Id, Today.AddDays(-3), Today), CancellationToken.None);

            result.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ShouldSkipInactiveSupplements()
        {
            var profile = await CreateProfileAsync();
            var supplement = await AddSupplementAsync(profile.Id, "Magnez", slotCount: 1);
            supplement.SetActive(false);
            await _context.SaveChangesAsync();

            var result = await _handler.Handle(
                new GetAdherenceQuery(profile.Id, Today.AddDays(-3), Today), CancellationToken.None);

            result.Items.Should().BeEmpty();
        }
    }
}
