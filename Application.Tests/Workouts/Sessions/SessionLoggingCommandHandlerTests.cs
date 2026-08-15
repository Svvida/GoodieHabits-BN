using Application.Workouts.Sessions.Commands.AddSessionExercise;
using Application.Workouts.Sessions.Commands.AddSet;
using Application.Workouts.Sessions.Commands.DeleteSet;
using Application.Workouts.Sessions.Commands.LogSession;
using Application.Workouts.Sessions.Commands.RemoveSessionExercise;
using Application.Workouts.Sessions.Commands.UpdateSet;
using Application.Workouts.Sessions.Dtos;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Workouts.Sessions
{
    /// <summary>
    /// The two logging paths — bulk replace and set-by-set — write the same rows and have to stay
    /// interchangeable, which is what most of these tests pin down.
    /// </summary>
    public class SessionLoggingCommandHandlerTests : TestBase<LogSessionCommandHandler>
    {
        private const int PullUps = 100;      // "Podciąganie nachwytem", Reps
        private const int BarbellRow = 111;   // "Wiosłowanie sztangą", RepsAndWeight

        private readonly LogSessionCommandHandler _logHandler;
        private readonly AddSessionExerciseCommandHandler _addExerciseHandler;
        private readonly RemoveSessionExerciseCommandHandler _removeExerciseHandler;
        private readonly AddSetCommandHandler _addSetHandler;
        private readonly UpdateSetCommandHandler _updateSetHandler;
        private readonly DeleteSetCommandHandler _deleteSetHandler;

        public SessionLoggingCommandHandlerTests()
        {
            _logHandler = new LogSessionCommandHandler(_unitOfWork, _mapper, _clockMock.Object);
            _addExerciseHandler = new AddSessionExerciseCommandHandler(_unitOfWork, _mapper, _clockMock.Object);
            _removeExerciseHandler = new RemoveSessionExerciseCommandHandler(_unitOfWork, _mapper);
            _addSetHandler = new AddSetCommandHandler(_unitOfWork, _mapper, _clockMock.Object);
            _updateSetHandler = new UpdateSetCommandHandler(_unitOfWork, _mapper);
            _deleteSetHandler = new DeleteSetCommandHandler(_unitOfWork, _mapper);
        }

        private async Task<UserProfile> CreateProfileAsync(string email = "user@test.com", string nickname = "user")
        {
            var account = await AddAccountAsync(email, "pass", nickname);
            return account.Profile;
        }

        private async Task<WorkoutSession> AddSessionAsync(int userProfileId)
        {
            var session = WorkoutSession.Start(
                userProfileId, "Trening", new DateOnly(2026, 8, 15), _fixedTestInstant.ToDateTimeUtc());
            _context.WorkoutSessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        private static SessionExerciseInput Row(int exerciseId, params SessionSetInput[] sets) =>
            new(exerciseId, Sets: sets);

        [Fact]
        public async Task Log_ShouldReplaceTheWholeTreeAndNumberEverything()
        {
            var profile = await CreateProfileAsync();
            var session = await AddSessionAsync(profile.Id);

            var result = await _logHandler.Handle(
                new LogSessionCommand(session.Id,
                [
                    Row(PullUps, new SessionSetInput(Reps: 10), new SessionSetInput(Reps: 8)),
                    Row(BarbellRow, new SessionSetInput(Reps: 8, Weight: 60m)),
                ],
                profile.Id),
                CancellationToken.None);

            result.Exercises.Select(e => e.Order).Should().Equal(0, 1);
            result.Exercises[0].ExerciseName.Should().Be("Podciąganie nachwytem");
            result.Exercises[0].Sets.Select(s => s.SetNumber).Should().Equal(1, 2);
            result.Exercises[1].Sets.Should().ContainSingle();
            result.Totals.SetCount.Should().Be(3);
            result.Totals.TotalReps.Should().Be(26);
            result.Totals.TotalVolume.Should().Be(480m);
        }

        [Fact]
        public async Task Log_ShouldBeIdempotentWhenTheSamePayloadIsRetried()
        {
            var profile = await CreateProfileAsync();
            var session = await AddSessionAsync(profile.Id);

            var payload = new List<SessionExerciseInput>
            {
                Row(PullUps, new SessionSetInput(Reps: 10)),
            };

            await _logHandler.Handle(new LogSessionCommand(session.Id, payload, profile.Id), CancellationToken.None);
            var result = await _logHandler.Handle(new LogSessionCommand(session.Id, payload, profile.Id), CancellationToken.None);

            // Full replacement, not append — a retry after a flaky gym connection must not double the log.
            result.Exercises.Should().ContainSingle();
            result.Exercises[0].Sets.Should().ContainSingle();
            _context.WorkoutSets.Should().HaveCount(1);
        }

        [Fact]
        public async Task Log_ShouldClearTheLogWhenGivenAnEmptyArray()
        {
            var profile = await CreateProfileAsync();
            var session = await AddSessionAsync(profile.Id);

            await _logHandler.Handle(
                new LogSessionCommand(session.Id, [Row(PullUps, new SessionSetInput(Reps: 10))], profile.Id),
                CancellationToken.None);

            var result = await _logHandler.Handle(
                new LogSessionCommand(session.Id, [], profile.Id), CancellationToken.None);

            result.Exercises.Should().BeEmpty();
            result.Totals.SetCount.Should().Be(0);
        }

        [Fact]
        public async Task Log_ShouldLeaveTheSessionUntouchedWhenAnExerciseIsUnknown()
        {
            var profile = await CreateProfileAsync();
            var session = await AddSessionAsync(profile.Id);

            await _logHandler.Handle(
                new LogSessionCommand(session.Id, [Row(PullUps, new SessionSetInput(Reps: 10))], profile.Id),
                CancellationToken.None);

            var act = () => _logHandler.Handle(
                new LogSessionCommand(session.Id, [Row(PullUps), Row(999_999)], profile.Id), CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();
            _context.WorkoutSessionExercises.Should().ContainSingle();
        }

        [Fact]
        public async Task Log_ShouldRejectASetMissingWhatItsMetricRequires()
        {
            var profile = await CreateProfileAsync();
            var session = await AddSessionAsync(profile.Id);

            // BarbellRow is RepsAndWeight; a set without weight cannot be logged against it.
            var act = () => _logHandler.Handle(
                new LogSessionCommand(session.Id, [Row(BarbellRow, new SessionSetInput(Reps: 8))], profile.Id),
                CancellationToken.None);

            await act.Should().ThrowAsync<InvalidArgumentException>();
        }

        [Fact]
        public async Task Log_ShouldNotFindAnotherUsersSession()
        {
            var owner = await CreateProfileAsync("a@test.com", "aaa");
            var stranger = await CreateProfileAsync("b@test.com", "bbb");
            var session = await AddSessionAsync(owner.Id);

            var act = () => _logHandler.Handle(
                new LogSessionCommand(session.Id, [], stranger.Id), CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task AddSet_ShouldAppendAndNumberSequentially()
        {
            var profile = await CreateProfileAsync();
            var session = await AddSessionAsync(profile.Id);

            var logged = await _addExerciseHandler.Handle(
                new AddSessionExerciseCommand(session.Id, new SessionExerciseInput(BarbellRow), profile.Id),
                CancellationToken.None);

            var entryId = logged.Exercises.Single().Id;

            await _addSetHandler.Handle(
                new AddSetCommand(session.Id, entryId, new SessionSetInput(Reps: 8, Weight: 60m), profile.Id),
                CancellationToken.None);

            var result = await _addSetHandler.Handle(
                new AddSetCommand(session.Id, entryId, new SessionSetInput(Reps: 6, Weight: 65m), profile.Id),
                CancellationToken.None);

            result.Exercises.Single().Sets.Select(s => s.SetNumber).Should().Equal(1, 2);
            result.Exercises.Single().Sets.Last().Weight.Should().Be(65m);
        }

        [Fact]
        public async Task AddSet_ShouldReturnTheEstimatedOneRepMax()
        {
            var profile = await CreateProfileAsync();
            var session = await AddSessionAsync(profile.Id);

            var logged = await _addExerciseHandler.Handle(
                new AddSessionExerciseCommand(session.Id, new SessionExerciseInput(BarbellRow), profile.Id),
                CancellationToken.None);

            var result = await _addSetHandler.Handle(
                new AddSetCommand(session.Id, logged.Exercises.Single().Id,
                    new SessionSetInput(Reps: 5, Weight: 100m), profile.Id),
                CancellationToken.None);

            // Epley: 100 × (1 + 5/30) = 116.67
            result.Exercises.Single().Sets.Single().EstimatedOneRepMax.Should().Be(116.67m);
        }

        [Fact]
        public async Task AddSet_ShouldRejectAnEntryFromAnotherSession()
        {
            var profile = await CreateProfileAsync();
            var first = await AddSessionAsync(profile.Id);

            var logged = await _addExerciseHandler.Handle(
                new AddSessionExerciseCommand(first.Id, new SessionExerciseInput(PullUps), profile.Id),
                CancellationToken.None);

            var second = WorkoutSession.Start(
                profile.Id, "Inny", new DateOnly(2026, 8, 16), _fixedTestInstant.ToDateTimeUtc());
            _context.WorkoutSessions.Add(second);
            await _context.SaveChangesAsync();

            var act = () => _addSetHandler.Handle(
                new AddSetCommand(second.Id, logged.Exercises.Single().Id, new SessionSetInput(Reps: 8), profile.Id),
                CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task UpdateSet_ShouldCorrectAnAlreadyLoggedSet()
        {
            var profile = await CreateProfileAsync();
            var session = await AddSessionAsync(profile.Id);

            var logged = await _logHandler.Handle(
                new LogSessionCommand(session.Id, [Row(BarbellRow, new SessionSetInput(Reps: 8, Weight: 60m))], profile.Id),
                CancellationToken.None);

            var entry = logged.Exercises.Single();

            var result = await _updateSetHandler.Handle(
                new UpdateSetCommand(session.Id, entry.Id, entry.Sets.Single().Id,
                    new SessionSetInput(Reps: 6, Weight: 70m, Rpe: 8.5m), profile.Id),
                CancellationToken.None);

            var set = result.Exercises.Single().Sets.Single();
            set.Reps.Should().Be(6);
            set.Weight.Should().Be(70m);
            set.Rpe.Should().Be(8.5m);
            set.SetNumber.Should().Be(1);
        }

        [Fact]
        public async Task UpdateSet_ShouldStillEnforceTheEntrysMetric()
        {
            var profile = await CreateProfileAsync();
            var session = await AddSessionAsync(profile.Id);

            var logged = await _logHandler.Handle(
                new LogSessionCommand(session.Id, [Row(BarbellRow, new SessionSetInput(Reps: 8, Weight: 60m))], profile.Id),
                CancellationToken.None);

            var entry = logged.Exercises.Single();

            var act = () => _updateSetHandler.Handle(
                new UpdateSetCommand(session.Id, entry.Id, entry.Sets.Single().Id,
                    new SessionSetInput(Reps: 6), profile.Id),
                CancellationToken.None);

            await act.Should().ThrowAsync<InvalidArgumentException>();
        }

        [Fact]
        public async Task DeleteSet_ShouldRenumberWhatIsLeft()
        {
            var profile = await CreateProfileAsync();
            var session = await AddSessionAsync(profile.Id);

            var logged = await _logHandler.Handle(
                new LogSessionCommand(session.Id,
                [
                    Row(PullUps,
                        new SessionSetInput(Reps: 10),
                        new SessionSetInput(Reps: 8),
                        new SessionSetInput(Reps: 6)),
                ],
                profile.Id),
                CancellationToken.None);

            var entry = logged.Exercises.Single();

            var result = await _deleteSetHandler.Handle(
                new DeleteSetCommand(session.Id, entry.Id, entry.Sets.First().Id, profile.Id),
                CancellationToken.None);

            result.Exercises.Single().Sets.Select(s => s.SetNumber).Should().Equal(1, 2);
            result.Exercises.Single().Sets.Select(s => s.Reps).Should().Equal(8, 6);
        }

        [Fact]
        public async Task RemoveExercise_ShouldRenumberTheRemainingPositions()
        {
            var profile = await CreateProfileAsync();
            var session = await AddSessionAsync(profile.Id);

            var logged = await _logHandler.Handle(
                new LogSessionCommand(session.Id, [Row(PullUps), Row(BarbellRow)], profile.Id),
                CancellationToken.None);

            var result = await _removeExerciseHandler.Handle(
                new RemoveSessionExerciseCommand(session.Id, logged.Exercises.First().Id, profile.Id),
                CancellationToken.None);

            result.Exercises.Should().ContainSingle();
            result.Exercises[0].Order.Should().Be(0);
            result.Exercises[0].ExerciseName.Should().Be("Wiosłowanie sztangą");
        }

        [Fact]
        public async Task AddExercise_ShouldAcceptAnExerciseTogetherWithItsSets()
        {
            var profile = await CreateProfileAsync();
            var session = await AddSessionAsync(profile.Id);

            var result = await _addExerciseHandler.Handle(
                new AddSessionExerciseCommand(session.Id,
                    new SessionExerciseInput(PullUps, Sets: [new SessionSetInput(Reps: 10), new SessionSetInput(Reps: 8)]),
                    profile.Id),
                CancellationToken.None);

            result.Exercises.Single().Sets.Should().HaveCount(2);
            result.Exercises.Single().MetricType.Should().Be(ExerciseMetricEnum.Reps);
        }

        [Fact]
        public async Task AddExercise_ShouldRejectAnUnknownExercise()
        {
            var profile = await CreateProfileAsync();
            var session = await AddSessionAsync(profile.Id);

            var act = () => _addExerciseHandler.Handle(
                new AddSessionExerciseCommand(session.Id, new SessionExerciseInput(999_999), profile.Id),
                CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
