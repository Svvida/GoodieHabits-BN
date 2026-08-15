using Application.Supplements.Catalog.Commands.AddSupplementSlot;
using Application.Supplements.Catalog.Commands.CreateSupplement;
using Application.Supplements.Catalog.Commands.DeleteSupplement;
using Application.Supplements.Catalog.Commands.DeleteSupplementSlot;
using Application.Supplements.Catalog.Commands.SetSupplementActive;
using Application.Supplements.Catalog.Commands.UpdateSupplement;
using Application.Supplements.Catalog.Commands.UpdateSupplementSlot;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Supplements.Catalog
{
    public class SupplementCatalogCommandHandlerTests : TestBase<CreateSupplementCommandHandler>
    {
        private readonly CreateSupplementCommandHandler _createHandler;
        private readonly UpdateSupplementCommandHandler _updateHandler;
        private readonly DeleteSupplementCommandHandler _deleteHandler;
        private readonly SetSupplementActiveCommandHandler _activeHandler;
        private readonly AddSupplementSlotCommandHandler _addSlotHandler;
        private readonly UpdateSupplementSlotCommandHandler _updateSlotHandler;
        private readonly DeleteSupplementSlotCommandHandler _deleteSlotHandler;

        public SupplementCatalogCommandHandlerTests()
        {
            _createHandler = new CreateSupplementCommandHandler(_unitOfWork, _mapper);
            _updateHandler = new UpdateSupplementCommandHandler(_unitOfWork, _mapper);
            _deleteHandler = new DeleteSupplementCommandHandler(_unitOfWork);
            _activeHandler = new SetSupplementActiveCommandHandler(_unitOfWork, _mapper);
            _addSlotHandler = new AddSupplementSlotCommandHandler(_unitOfWork, _mapper);
            _updateSlotHandler = new UpdateSupplementSlotCommandHandler(_unitOfWork, _mapper);
            _deleteSlotHandler = new DeleteSupplementSlotCommandHandler(_unitOfWork, _mapper);
        }

        private async Task<UserProfile> CreateProfileAsync(string email = "user@test.com", string nickname = "user")
        {
            var account = await AddAccountAsync(email, "pass", nickname);
            return account.Profile;
        }

        private Task<Application.Supplements.Catalog.Dtos.SupplementDto> CreateMagnesiumAsync(int userProfileId) =>
            _createHandler.Handle(
                new CreateSupplementCommand("Magnez", SupplementUnitEnum.Capsule, 1m, null, "#10B981", "leaf-outline", userProfileId),
                CancellationToken.None);

        [Fact]
        public async Task Create_ShouldStoreTheSupplementAsActiveWithNoSlots()
        {
            var profile = await CreateProfileAsync();

            var result = await CreateMagnesiumAsync(profile.Id);

            result.Name.Should().Be("Magnez");
            result.Unit.Should().Be(SupplementUnitEnum.Capsule);
            result.DefaultAmount.Should().Be(1m);
            result.IsActive.Should().BeTrue();
            result.Slots.Should().BeEmpty();
        }

        [Fact]
        public async Task Create_ShouldRejectADuplicateName()
        {
            var profile = await CreateProfileAsync();
            await CreateMagnesiumAsync(profile.Id);

            var act = () => CreateMagnesiumAsync(profile.Id);

            await act.Should().ThrowAsync<ConflictException>();
        }

        [Fact]
        public async Task AddSlot_ShouldLetOneSupplementCarrySeveralDosesADay()
        {
            var profile = await CreateProfileAsync();
            var supplement = await CreateMagnesiumAsync(profile.Id);

            await _addSlotHandler.Handle(
                new AddSupplementSlotCommand(supplement.Id, SupplementTimingEnum.Morning, 1m, null, null, null, profile.Id),
                CancellationToken.None);

            var result = await _addSlotHandler.Handle(
                new AddSupplementSlotCommand(supplement.Id, SupplementTimingEnum.Night, 2m, null, null, "przed snem", profile.Id),
                CancellationToken.None);

            result.Slots.Should().HaveCount(2);
            result.Slots.Should().Contain(s => s.Timing == SupplementTimingEnum.Night && s.Amount == 2m);
        }

        [Fact]
        public async Task AddSlot_ShouldCarryAWorkoutRelativeOffset()
        {
            var profile = await CreateProfileAsync();
            var supplement = await CreateMagnesiumAsync(profile.Id);

            var result = await _addSlotHandler.Handle(
                new AddSupplementSlotCommand(supplement.Id, SupplementTimingEnum.PreWorkout, 5m, null, -30, null, profile.Id),
                CancellationToken.None);

            // "30 min przed treningiem"
            result.Slots.Single().OffsetMinutes.Should().Be(-30);
        }

        [Fact]
        public async Task AddSlot_ShouldRejectACustomTimingWithoutAClockTime()
        {
            var profile = await CreateProfileAsync();
            var supplement = await CreateMagnesiumAsync(profile.Id);

            var act = () => _addSlotHandler.Handle(
                new AddSupplementSlotCommand(supplement.Id, SupplementTimingEnum.Custom, 1m, null, null, null, profile.Id),
                CancellationToken.None);

            await act.Should().ThrowAsync<InvalidArgumentException>();
        }

        [Fact]
        public async Task UpdateSlot_ShouldRejectASlotFromAnotherSupplement()
        {
            var profile = await CreateProfileAsync();
            var magnesium = await CreateMagnesiumAsync(profile.Id);
            var creatine = await _createHandler.Handle(
                new CreateSupplementCommand("Kreatyna", SupplementUnitEnum.Gram, 5m, null, null, null, profile.Id),
                CancellationToken.None);

            var withSlot = await _addSlotHandler.Handle(
                new AddSupplementSlotCommand(creatine.Id, SupplementTimingEnum.PreWorkout, 5m, null, null, null, profile.Id),
                CancellationToken.None);

            var act = () => _updateSlotHandler.Handle(
                new UpdateSupplementSlotCommand(
                    magnesium.Id, withSlot.Slots.Single().Id, SupplementTimingEnum.Morning, 1m, null, null, null, profile.Id),
                CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task DeleteSlot_ShouldKeepDosesAlreadyTakenAgainstIt()
        {
            var profile = await CreateProfileAsync();
            var supplement = await CreateMagnesiumAsync(profile.Id);

            var withSlot = await _addSlotHandler.Handle(
                new AddSupplementSlotCommand(supplement.Id, SupplementTimingEnum.Morning, 1m, null, null, null, profile.Id),
                CancellationToken.None);

            var slotId = withSlot.Slots.Single().Id;
            var entity = await _context.Supplements.FindAsync(supplement.Id);
            var slot = _context.SupplementScheduleSlots.First(s => s.Id == slotId);

            var intake = SupplementIntake.Create(
                entity!, new DateOnly(2026, 8, 15), _fixedTestInstant.ToDateTimeUtc(), slot);
            _context.SupplementIntakes.Add(intake);
            await _context.SaveChangesAsync();

            var result = await _deleteSlotHandler.Handle(
                new DeleteSupplementSlotCommand(supplement.Id, slotId, profile.Id), CancellationToken.None);

            result.Slots.Should().BeEmpty();

            // The plan is gone; what was swallowed is not — it becomes an ordinary ad-hoc record.
            var saved = await _context.SupplementIntakes.FindAsync(intake.Id);
            saved.Should().NotBeNull();
            saved!.ScheduleSlotId.Should().BeNull();
            saved.IsAdHoc.Should().BeTrue();
        }

        [Fact]
        public async Task Delete_ShouldRefuseOnceSomethingHasBeenLogged()
        {
            var profile = await CreateProfileAsync();
            var supplement = await CreateMagnesiumAsync(profile.Id);

            var entity = await _context.Supplements.FindAsync(supplement.Id);
            _context.SupplementIntakes.Add(SupplementIntake.Create(
                entity!, new DateOnly(2026, 8, 15), _fixedTestInstant.ToDateTimeUtc()));
            await _context.SaveChangesAsync();

            var act = () => _deleteHandler.Handle(
                new DeleteSupplementCommand(supplement.Id, profile.Id), CancellationToken.None);

            var exception = await act.Should().ThrowAsync<ConflictException>();
            exception.Which.Message.Should().Contain("Deactivate");
        }

        [Fact]
        public async Task Delete_ShouldSucceedWhenNothingWasEverLogged()
        {
            var profile = await CreateProfileAsync();
            var supplement = await CreateMagnesiumAsync(profile.Id);

            await _deleteHandler.Handle(
                new DeleteSupplementCommand(supplement.Id, profile.Id), CancellationToken.None);

            (await _context.Supplements.FindAsync(supplement.Id)).Should().BeNull();
        }

        [Fact]
        public async Task SetActive_ShouldRetireWithoutLosingTheSchedule()
        {
            var profile = await CreateProfileAsync();
            var supplement = await CreateMagnesiumAsync(profile.Id);
            await _addSlotHandler.Handle(
                new AddSupplementSlotCommand(supplement.Id, SupplementTimingEnum.Morning, 1m, null, null, null, profile.Id),
                CancellationToken.None);

            var result = await _activeHandler.Handle(
                new SetSupplementActiveCommand(supplement.Id, false, profile.Id), CancellationToken.None);

            result.IsActive.Should().BeFalse();
            result.Slots.Should().ContainSingle();
        }

        [Fact]
        public async Task Update_ShouldNotFindAnotherUsersSupplement()
        {
            var owner = await CreateProfileAsync("a@test.com", "aaa");
            var stranger = await CreateProfileAsync("b@test.com", "bbb");
            var supplement = await CreateMagnesiumAsync(owner.Id);

            var act = () => _updateHandler.Handle(
                new UpdateSupplementCommand(
                    supplement.Id, "Przejęte", SupplementUnitEnum.Gram, null, null, null, null, stranger.Id),
                CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
