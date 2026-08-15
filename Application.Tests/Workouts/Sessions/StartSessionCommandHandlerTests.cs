using Application.Workouts.Sessions.Commands.StartSession;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Workouts.Sessions
{
    public class StartSessionCommandHandlerTests : TestBase<StartSessionCommandHandler>
    {
        private readonly StartSessionCommandHandler _handler;

        public StartSessionCommandHandlerTests()
        {
            _handler = new StartSessionCommandHandler(_unitOfWork, _mapper, _clockMock.Object);
        }

        private async Task<UserProfile> CreateProfileAsync(
            string email = "user@test.com", string nickname = "user", string timeZone = "Etc/UTC")
        {
            var account = await AddAccountAsync(email, "pass", nickname, timeZone);
            return account.Profile;
        }

        private async Task<WorkoutRoutine> AddRoutineAsync(int userProfileId, params int[] exerciseIds)
        {
            var routine = WorkoutRoutine.Create(userProfileId, "Push A");
            routine.ReplaceExercises([.. exerciseIds.Select(id => WorkoutRoutineExercise.Create(id, targetReps: 8))]);
            _context.WorkoutRoutines.Add(routine);
            await _context.SaveChangesAsync();
            return routine;
        }

        [Fact]
        public async Task Handle_ShouldStartAnAdHocSessionInProgress()
        {
            var profile = await CreateProfileAsync();

            var result = await _handler.Handle(
                new StartSessionCommand(null, "Trening", new DateOnly(2026, 8, 15), null, profile.Id),
                CancellationToken.None);

            result.Status.Should().Be(WorkoutSessionStatusEnum.InProgress);
            result.Name.Should().Be("Trening");
            result.RoutineId.Should().BeNull();
            result.PerformedOn.Should().Be(new DateOnly(2026, 8, 15));
            result.CompletedAt.Should().BeNull();
            result.Exercises.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ShouldDefaultPerformedOnToTheUsersLocalToday()
        {
            // The fixed clock reads 2023-10-26 10:00 UTC; in Auckland that is already the 26th at 23:00, and in
            // Los Angeles still the 26th at 03:00 — the point is that the date comes from the profile's zone,
            // not from UTC arithmetic in the handler.
            var profile = await CreateProfileAsync(timeZone: "Pacific/Auckland");

            var result = await _handler.Handle(
                new StartSessionCommand(null, "Trening", null, null, profile.Id), CancellationToken.None);

            result.PerformedOn.Should().Be(profile.LocalDateOn(_fixedTestInstant.ToDateTimeUtc()));
        }

        [Fact]
        public async Task Handle_ShouldMaterializeTheRoutineIntoTheSession()
        {
            var profile = await CreateProfileAsync();
            // System exercises 100 "Podciąganie nachwytem" (Reps) and 111 "Wiosłowanie sztangą" (RepsAndWeight).
            var routine = await AddRoutineAsync(profile.Id, 100, 111);

            var result = await _handler.Handle(
                new StartSessionCommand(routine.Id, null, new DateOnly(2026, 8, 15), null, profile.Id),
                CancellationToken.None);

            result.RoutineId.Should().Be(routine.Id);
            result.Name.Should().Be("Push A");
            result.Exercises.Select(e => e.Order).Should().Equal(0, 1);
            result.Exercises.Select(e => e.ExerciseName).Should().Equal("Podciąganie nachwytem", "Wiosłowanie sztangą");
            result.Exercises.Select(e => e.MetricType).Should()
                .Equal(ExerciseMetricEnum.Reps, ExerciseMetricEnum.RepsAndWeight);
            result.Exercises.Should().OnlyContain(e => e.TargetReps == 8);
            result.Exercises.Should().OnlyContain(e => e.Sets.Count == 0);
        }

        [Fact]
        public async Task Handle_ShouldLetTheCallerOverrideTheRoutineName()
        {
            var profile = await CreateProfileAsync();
            var routine = await AddRoutineAsync(profile.Id, 100);

            var result = await _handler.Handle(
                new StartSessionCommand(routine.Id, "Push A — lekki", new DateOnly(2026, 8, 15), null, profile.Id),
                CancellationToken.None);

            result.Name.Should().Be("Push A — lekki");
        }

        [Fact]
        public async Task Handle_ShouldRejectASecondSessionAndNameTheActiveOne()
        {
            var profile = await CreateProfileAsync();

            var first = await _handler.Handle(
                new StartSessionCommand(null, "Trening", new DateOnly(2026, 8, 15), null, profile.Id),
                CancellationToken.None);

            var act = () => _handler.Handle(
                new StartSessionCommand(null, "Drugi", new DateOnly(2026, 8, 15), null, profile.Id),
                CancellationToken.None);

            var exception = await act.Should().ThrowAsync<ConflictException>();
            exception.Which.Message.Should().Contain(first.Id.ToString());
        }

        [Fact]
        public async Task Handle_ShouldAllowANewSessionOnceTheLastOneIsFinished()
        {
            var profile = await CreateProfileAsync();

            var first = await _handler.Handle(
                new StartSessionCommand(null, "Trening", new DateOnly(2026, 8, 15), null, profile.Id),
                CancellationToken.None);

            var session = await _context.WorkoutSessions.FindAsync(first.Id);
            session!.Complete(_fixedTestInstant.ToDateTimeUtc().AddMinutes(40));
            await _context.SaveChangesAsync();

            var second = await _handler.Handle(
                new StartSessionCommand(null, "Drugi", new DateOnly(2026, 8, 16), null, profile.Id),
                CancellationToken.None);

            second.Status.Should().Be(WorkoutSessionStatusEnum.InProgress);
        }

        [Fact]
        public async Task Handle_ShouldRejectAnUnknownRoutine()
        {
            var profile = await CreateProfileAsync();

            var act = () => _handler.Handle(
                new StartSessionCommand(999_999, null, new DateOnly(2026, 8, 15), null, profile.Id),
                CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handle_ShouldRejectAnotherUsersRoutine()
        {
            var owner = await CreateProfileAsync("a@test.com", "aaa");
            var stranger = await CreateProfileAsync("b@test.com", "bbb");
            var routine = await AddRoutineAsync(owner.Id, 100);

            var act = () => _handler.Handle(
                new StartSessionCommand(routine.Id, null, new DateOnly(2026, 8, 15), null, stranger.Id),
                CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
