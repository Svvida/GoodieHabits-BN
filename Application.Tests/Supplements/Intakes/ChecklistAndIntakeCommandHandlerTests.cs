using Application.Supplements.Catalog.Commands.AddSupplementSlot;
using Application.Supplements.Catalog.Commands.CreateSupplement;
using Application.Supplements.Catalog.Commands.SetSupplementActive;
using Application.Supplements.Catalog.Dtos;
using Application.Supplements.Intakes.Commands.DeleteIntake;
using Application.Supplements.Intakes.Commands.LogAdHocIntake;
using Application.Supplements.Intakes.Commands.ToggleIntake;
using Application.Supplements.Intakes.Queries.GetChecklist;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Supplements.Intakes
{
    /// <summary>
    /// The checkbox is the heart of this module, so most of these tests pin down its idempotency and the two
    /// places history must survive a change of plan.
    /// </summary>
    public class ChecklistAndIntakeCommandHandlerTests : TestBase<ToggleIntakeCommandHandler>
    {
        private static readonly DateOnly Today = new(2026, 8, 15);

        private readonly CreateSupplementCommandHandler _createHandler;
        private readonly AddSupplementSlotCommandHandler _addSlotHandler;
        private readonly SetSupplementActiveCommandHandler _activeHandler;
        private readonly ToggleIntakeCommandHandler _toggleHandler;
        private readonly LogAdHocIntakeCommandHandler _adHocHandler;
        private readonly DeleteIntakeCommandHandler _deleteIntakeHandler;
        private readonly GetChecklistQueryHandler _checklistHandler;

        public ChecklistAndIntakeCommandHandlerTests()
        {
            _createHandler = new CreateSupplementCommandHandler(_unitOfWork, _mapper);
            _addSlotHandler = new AddSupplementSlotCommandHandler(_unitOfWork, _mapper);
            _activeHandler = new SetSupplementActiveCommandHandler(_unitOfWork, _mapper);
            _toggleHandler = new ToggleIntakeCommandHandler(_unitOfWork, _clockMock.Object);
            _adHocHandler = new LogAdHocIntakeCommandHandler(_unitOfWork, _clockMock.Object);
            _deleteIntakeHandler = new DeleteIntakeCommandHandler(_unitOfWork);
            _checklistHandler = new GetChecklistQueryHandler(_unitOfWork);
        }

        private async Task<UserProfile> CreateProfileAsync(string email = "user@test.com", string nickname = "user")
        {
            var account = await AddAccountAsync(email, "pass", nickname);
            return account.Profile;
        }

        private async Task<SupplementDto> CreateWithSlotAsync(
            int userProfileId,
            string name,
            SupplementTimingEnum timing,
            decimal amount,
            int? offsetMinutes = null)
        {
            var supplement = await _createHandler.Handle(
                new CreateSupplementCommand(name, SupplementUnitEnum.Capsule, 1m, null, null, null, userProfileId),
                CancellationToken.None);

            return await _addSlotHandler.Handle(
                new AddSupplementSlotCommand(supplement.Id, timing, amount, null, offsetMinutes, null, userProfileId),
                CancellationToken.None);
        }

        [Fact]
        public async Task Checklist_ShouldListEveryPlannedDoseAsUntickedByDefault()
        {
            var profile = await CreateProfileAsync();
            await CreateWithSlotAsync(profile.Id, "Magnez", SupplementTimingEnum.Morning, 1m);
            await CreateWithSlotAsync(profile.Id, "Kreatyna", SupplementTimingEnum.PreWorkout, 5m, -30);

            var result = await _checklistHandler.Handle(
                new GetChecklistQuery(profile.Id, Today, null), CancellationToken.None);

            result.Date.Should().Be(Today);
            result.Items.Should().HaveCount(2);
            result.Items.Should().OnlyContain(i => !i.Taken && i.IntakeId == null);
            result.AdHoc.Should().BeEmpty();
        }

        [Fact]
        public async Task Checklist_ShouldFilterToTheInTrainingSliceWhenAskedTo()
        {
            var profile = await CreateProfileAsync();
            await CreateWithSlotAsync(profile.Id, "Magnez", SupplementTimingEnum.Morning, 1m);
            var creatine = await CreateWithSlotAsync(profile.Id, "Kreatyna", SupplementTimingEnum.PreWorkout, 5m, -30);

            var result = await _checklistHandler.Handle(
                new GetChecklistQuery(profile.Id, Today, [SupplementTimingEnum.PreWorkout, SupplementTimingEnum.PostWorkout]),
                CancellationToken.None);

            // This filter is the entire integration between the supplements and workouts modules.
            result.Items.Should().ContainSingle();
            result.Items[0].SupplementId.Should().Be(creatine.Id);
            result.Items[0].OffsetMinutes.Should().Be(-30);
        }

        [Fact]
        public async Task Checklist_ShouldHideInactiveSupplements()
        {
            var profile = await CreateProfileAsync();
            var supplement = await CreateWithSlotAsync(profile.Id, "Magnez", SupplementTimingEnum.Morning, 1m);

            await _activeHandler.Handle(
                new SetSupplementActiveCommand(supplement.Id, false, profile.Id), CancellationToken.None);

            var result = await _checklistHandler.Handle(
                new GetChecklistQuery(profile.Id, Today, null), CancellationToken.None);

            result.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task Toggle_ShouldTickTheDoseAndFallBackToThePlannedAmount()
        {
            var profile = await CreateProfileAsync();
            var supplement = await CreateWithSlotAsync(profile.Id, "Magnez", SupplementTimingEnum.Morning, 2m);
            var slotId = supplement.Slots.Single().Id;

            var result = await _toggleHandler.Handle(
                new ToggleIntakeCommand(supplement.Id, slotId, Today, true, null, null, profile.Id),
                CancellationToken.None);

            var item = result.Items.Single();
            item.Taken.Should().BeTrue();
            item.TakenAmount.Should().Be(2m);
            item.TakenAt.Should().Be(_fixedTestInstant.ToDateTimeUtc());
            item.IntakeId.Should().NotBeNull();
        }

        [Fact]
        public async Task Toggle_ShouldBeIdempotentWhenTappedTwice()
        {
            var profile = await CreateProfileAsync();
            var supplement = await CreateWithSlotAsync(profile.Id, "Magnez", SupplementTimingEnum.Morning, 1m);
            var slotId = supplement.Slots.Single().Id;

            var command = new ToggleIntakeCommand(supplement.Id, slotId, Today, true, null, null, profile.Id);

            await _toggleHandler.Handle(command, CancellationToken.None);
            var result = await _toggleHandler.Handle(command, CancellationToken.None);

            // A phone taps fast, offline, and sometimes twice — one dose either way.
            result.Items.Single().Taken.Should().BeTrue();
            _context.SupplementIntakes.Should().ContainSingle();
        }

        [Fact]
        public async Task Toggle_ShouldUntickAndRemoveTheDose()
        {
            var profile = await CreateProfileAsync();
            var supplement = await CreateWithSlotAsync(profile.Id, "Magnez", SupplementTimingEnum.Morning, 1m);
            var slotId = supplement.Slots.Single().Id;

            await _toggleHandler.Handle(
                new ToggleIntakeCommand(supplement.Id, slotId, Today, true, null, null, profile.Id),
                CancellationToken.None);

            var result = await _toggleHandler.Handle(
                new ToggleIntakeCommand(supplement.Id, slotId, Today, false, null, null, profile.Id),
                CancellationToken.None);

            result.Items.Single().Taken.Should().BeFalse();
            _context.SupplementIntakes.Should().BeEmpty();
        }

        [Fact]
        public async Task Toggle_ShouldTreatUntickingSomethingNeverTickedAsANoOp()
        {
            var profile = await CreateProfileAsync();
            var supplement = await CreateWithSlotAsync(profile.Id, "Magnez", SupplementTimingEnum.Morning, 1m);

            var result = await _toggleHandler.Handle(
                new ToggleIntakeCommand(supplement.Id, supplement.Slots.Single().Id, Today, false, null, null, profile.Id),
                CancellationToken.None);

            result.Items.Single().Taken.Should().BeFalse();
        }

        [Fact]
        public async Task Toggle_ShouldKeepTheSameSlotSeparateAcrossDays()
        {
            var profile = await CreateProfileAsync();
            var supplement = await CreateWithSlotAsync(profile.Id, "Magnez", SupplementTimingEnum.Morning, 1m);
            var slotId = supplement.Slots.Single().Id;

            await _toggleHandler.Handle(
                new ToggleIntakeCommand(supplement.Id, slotId, Today, true, null, null, profile.Id),
                CancellationToken.None);

            var yesterday = await _toggleHandler.Handle(
                new ToggleIntakeCommand(supplement.Id, slotId, Today.AddDays(-1), true, null, null, profile.Id),
                CancellationToken.None);

            yesterday.Date.Should().Be(Today.AddDays(-1));
            yesterday.Items.Single().Taken.Should().BeTrue();
            _context.SupplementIntakes.Should().HaveCount(2);
        }

        [Fact]
        public async Task Toggle_ShouldRecordTheTrainingSessionWhenTickedFromTheWorkoutScreen()
        {
            var profile = await CreateProfileAsync();
            var supplement = await CreateWithSlotAsync(profile.Id, "Kreatyna", SupplementTimingEnum.PreWorkout, 5m, -30);

            var session = WorkoutSession.Start(
                profile.Id, "Trening", Today, _fixedTestInstant.ToDateTimeUtc());
            _context.WorkoutSessions.Add(session);
            await _context.SaveChangesAsync();

            var result = await _toggleHandler.Handle(
                new ToggleIntakeCommand(supplement.Id, supplement.Slots.Single().Id, Today, true, null, session.Id, profile.Id),
                CancellationToken.None);

            result.Items.Single().WorkoutSessionId.Should().Be(session.Id);
        }

        [Fact]
        public async Task Toggle_ShouldRejectASlotFromAnotherSupplement()
        {
            var profile = await CreateProfileAsync();
            var magnesium = await CreateWithSlotAsync(profile.Id, "Magnez", SupplementTimingEnum.Morning, 1m);
            var creatine = await CreateWithSlotAsync(profile.Id, "Kreatyna", SupplementTimingEnum.PreWorkout, 5m);

            var act = () => _toggleHandler.Handle(
                new ToggleIntakeCommand(magnesium.Id, creatine.Slots.Single().Id, Today, true, null, null, profile.Id),
                CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Toggle_ShouldNotFindAnotherUsersSupplement()
        {
            var owner = await CreateProfileAsync("a@test.com", "aaa");
            var stranger = await CreateProfileAsync("b@test.com", "bbb");
            var supplement = await CreateWithSlotAsync(owner.Id, "Magnez", SupplementTimingEnum.Morning, 1m);

            var act = () => _toggleHandler.Handle(
                new ToggleIntakeCommand(supplement.Id, supplement.Slots.Single().Id, Today, true, null, null, stranger.Id),
                CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task AdHoc_ShouldRecordAnUnplannedDoseAndStayRepeatable()
        {
            var profile = await CreateProfileAsync();
            var supplement = await _createHandler.Handle(
                new CreateSupplementCommand("Witamina C", SupplementUnitEnum.Tablet, 1m, null, null, null, profile.Id),
                CancellationToken.None);

            await _adHocHandler.Handle(
                new LogAdHocIntakeCommand(supplement.Id, Today, null, null, profile.Id), CancellationToken.None);

            var result = await _adHocHandler.Handle(
                new LogAdHocIntakeCommand(supplement.Id, Today, 2m, null, profile.Id), CancellationToken.None);

            // Unlike the checkbox, repeating an unplanned dose is a real event and must not collapse.
            result.AdHoc.Should().HaveCount(2);
            result.AdHoc.Should().OnlyContain(i => i.ScheduleSlotId == null);
            result.AdHoc.Last().Amount.Should().Be(2m);
            result.AdHoc.Should().OnlyContain(i => i.SupplementName == "Witamina C");
        }

        [Fact]
        public async Task AdHoc_ShouldWorkForAnInactiveSupplement()
        {
            var profile = await CreateProfileAsync();
            var supplement = await _createHandler.Handle(
                new CreateSupplementCommand("Witamina C", SupplementUnitEnum.Tablet, 1m, null, null, null, profile.Id),
                CancellationToken.None);

            await _activeHandler.Handle(
                new SetSupplementActiveCommand(supplement.Id, false, profile.Id), CancellationToken.None);

            var result = await _adHocHandler.Handle(
                new LogAdHocIntakeCommand(supplement.Id, Today, null, null, profile.Id), CancellationToken.None);

            // Deactivating means "off my plan", not "I can never take this".
            result.AdHoc.Should().ContainSingle();
            result.AdHoc[0].SupplementName.Should().Be("Witamina C");
        }

        [Fact]
        public async Task AdHoc_ShouldRejectADoseWithNoAmountAnywhere()
        {
            var profile = await CreateProfileAsync();
            var supplement = await _createHandler.Handle(
                new CreateSupplementCommand("Witamina C", SupplementUnitEnum.Tablet, null, null, null, null, profile.Id),
                CancellationToken.None);

            var act = () => _adHocHandler.Handle(
                new LogAdHocIntakeCommand(supplement.Id, Today, null, null, profile.Id), CancellationToken.None);

            await act.Should().ThrowAsync<InvalidArgumentException>();
        }

        [Fact]
        public async Task DeleteIntake_ShouldUndoAnAdHocDose()
        {
            var profile = await CreateProfileAsync();
            var supplement = await _createHandler.Handle(
                new CreateSupplementCommand("Witamina C", SupplementUnitEnum.Tablet, 1m, null, null, null, profile.Id),
                CancellationToken.None);

            var logged = await _adHocHandler.Handle(
                new LogAdHocIntakeCommand(supplement.Id, Today, null, null, profile.Id), CancellationToken.None);

            var result = await _deleteIntakeHandler.Handle(
                new DeleteIntakeCommand(logged.AdHoc.Single().Id, profile.Id), CancellationToken.None);

            result.Date.Should().Be(Today);
            result.AdHoc.Should().BeEmpty();
        }

        [Fact]
        public async Task DeleteIntake_ShouldNotFindAnotherUsersDose()
        {
            var owner = await CreateProfileAsync("a@test.com", "aaa");
            var stranger = await CreateProfileAsync("b@test.com", "bbb");

            var supplement = await _createHandler.Handle(
                new CreateSupplementCommand("Witamina C", SupplementUnitEnum.Tablet, 1m, null, null, null, owner.Id),
                CancellationToken.None);

            var logged = await _adHocHandler.Handle(
                new LogAdHocIntakeCommand(supplement.Id, Today, null, null, owner.Id), CancellationToken.None);

            var act = () => _deleteIntakeHandler.Handle(
                new DeleteIntakeCommand(logged.AdHoc.Single().Id, stranger.Id), CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
