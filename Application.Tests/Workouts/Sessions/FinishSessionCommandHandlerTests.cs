using Application.Common;
using Application.Workouts.Sessions.Commands.AbandonSession;
using Application.Workouts.Sessions.Commands.DeleteSession;
using Application.Workouts.Sessions.Commands.FinishSession;
using Domain.Enums;
using Domain.Events.Workouts;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;
using MediatR;
using Moq;

namespace Application.Tests.Workouts.Sessions
{
    public class FinishSessionCommandHandlerTests : TestBase<FinishSessionCommandHandler>
    {
        private readonly FinishSessionCommandHandler _finishHandler;
        private readonly AbandonSessionCommandHandler _abandonHandler;
        private readonly DeleteSessionCommandHandler _deleteHandler;

        public FinishSessionCommandHandlerTests()
        {
            _finishHandler = new FinishSessionCommandHandler(
                _unitOfWork, _mapper, _clockMock.Object, _mediatorMock.Object);
            _abandonHandler = new AbandonSessionCommandHandler(_unitOfWork, _mapper, _clockMock.Object);
            _deleteHandler = new DeleteSessionCommandHandler(_unitOfWork);
        }

        private async Task<UserProfile> CreateProfileAsync(string email = "user@test.com", string nickname = "user")
        {
            var account = await AddAccountAsync(email, "pass", nickname);
            return account.Profile;
        }

        private async Task<WorkoutSession> AddSessionAsync(int userProfileId, bool withSets = true)
        {
            var startedAt = _fixedTestInstant.ToDateTimeUtc().AddMinutes(-45);
            var session = WorkoutSession.Start(userProfileId, "Trening", new DateOnly(2026, 8, 15), startedAt);

            if (withSets)
            {
                var exercise = Exercise.CreateSystem(
                    9001, "Wyciskanie", ExerciseMetricEnum.RepsAndWeight, MuscleGroupEnum.Chest, EquipmentEnum.Barbell);
                var entry = session.AddExercise(exercise);
                entry.AddSet(startedAt, reps: 8, weight: 60m);
                entry.AddSet(startedAt, reps: 8, weight: 60m);
            }

            _context.WorkoutSessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        [Fact]
        public async Task Finish_ShouldCloseTheSessionAndReportItsTotals()
        {
            var profile = await CreateProfileAsync();
            var session = await AddSessionAsync(profile.Id);

            var result = await _finishHandler.Handle(
                new FinishSessionCommand(session.Id, profile.Id), CancellationToken.None);

            result.Status.Should().Be(WorkoutSessionStatusEnum.Completed);
            result.CompletedAt.Should().Be(_fixedTestInstant.ToDateTimeUtc());
            result.DurationSeconds.Should().Be(45 * 60);
            result.Totals.SetCount.Should().Be(2);
            result.Totals.TotalVolume.Should().Be(960m);
        }

        [Fact]
        public async Task Finish_ShouldPublishTheGamificationHook()
        {
            var profile = await CreateProfileAsync();
            var session = await AddSessionAsync(profile.Id);

            await _finishHandler.Handle(new FinishSessionCommand(session.Id, profile.Id), CancellationToken.None);

            // Nothing consumes it yet — the point is that it is raised, so awarding XP later needs no migration.
            _mediatorMock.Verify(
                m => m.Publish(
                    It.Is<INotification>(n => n is DomainEventNotification<WorkoutSessionCompletedEvent>),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Finish_ShouldClearTheEventSoItCannotBeRepublished()
        {
            var profile = await CreateProfileAsync();
            var session = await AddSessionAsync(profile.Id);

            await _finishHandler.Handle(new FinishSessionCommand(session.Id, profile.Id), CancellationToken.None);

            var saved = await _context.WorkoutSessions.FindAsync(session.Id);
            saved!.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public async Task Finish_ShouldRejectASecondCompletion()
        {
            var profile = await CreateProfileAsync();
            var session = await AddSessionAsync(profile.Id);

            await _finishHandler.Handle(new FinishSessionCommand(session.Id, profile.Id), CancellationToken.None);

            var act = () => _finishHandler.Handle(
                new FinishSessionCommand(session.Id, profile.Id), CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>();
        }

        [Fact]
        public async Task Finish_ShouldNotFindAnotherUsersSession()
        {
            var owner = await CreateProfileAsync("a@test.com", "aaa");
            var stranger = await CreateProfileAsync("b@test.com", "bbb");
            var session = await AddSessionAsync(owner.Id);

            var act = () => _finishHandler.Handle(
                new FinishSessionCommand(session.Id, stranger.Id), CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Abandon_ShouldKeepTheRecordButRaiseNoEvent()
        {
            var profile = await CreateProfileAsync();
            var session = await AddSessionAsync(profile.Id);

            var result = await _abandonHandler.Handle(
                new AbandonSessionCommand(session.Id, profile.Id), CancellationToken.None);

            result.Status.Should().Be(WorkoutSessionStatusEnum.Abandoned);
            result.Exercises.Should().ContainSingle();

            // An abandoned session is not an achievement and must never feed gamification.
            _mediatorMock.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Abandon_ShouldRejectACompletedSession()
        {
            var profile = await CreateProfileAsync();
            var session = await AddSessionAsync(profile.Id);

            await _finishHandler.Handle(new FinishSessionCommand(session.Id, profile.Id), CancellationToken.None);

            var act = () => _abandonHandler.Handle(
                new AbandonSessionCommand(session.Id, profile.Id), CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>();
        }

        [Fact]
        public async Task Delete_ShouldRemoveTheSessionWithItsEntriesAndSets()
        {
            var profile = await CreateProfileAsync();
            var session = await AddSessionAsync(profile.Id);

            await _deleteHandler.Handle(new DeleteSessionCommand(session.Id, profile.Id), CancellationToken.None);

            (await _context.WorkoutSessions.FindAsync(session.Id)).Should().BeNull();
            _context.WorkoutSessionExercises.Should().BeEmpty();
            _context.WorkoutSets.Should().BeEmpty();
        }
    }
}
