using Application.Workouts.Analytics.Queries.GetExerciseHistory;
using Application.Workouts.Analytics.Queries.GetPersonalRecords;
using Application.Workouts.Analytics.Queries.GetWorkoutSummary;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Workouts.Analytics
{
    public class WorkoutAnalyticsQueryHandlerTests : TestBase<GetWorkoutSummaryQueryHandler>
    {
        private const int PullUps = 100;      // "Podciąganie nachwytem", Reps, Back
        private const int BarbellRow = 111;   // "Wiosłowanie sztangą", RepsAndWeight, Back
        private const int Bench = 11;         // "Wyciskanie sztangi leżąc", RepsAndWeight, Chest

        private static readonly DateOnly From = new(2026, 8, 1);
        private static readonly DateOnly To = new(2026, 8, 31);

        private readonly GetWorkoutSummaryQueryHandler _summaryHandler;
        private readonly GetExerciseHistoryQueryHandler _historyHandler;
        private readonly GetPersonalRecordsQueryHandler _recordsHandler;

        public WorkoutAnalyticsQueryHandlerTests()
        {
            _summaryHandler = new GetWorkoutSummaryQueryHandler(_unitOfWork);
            _historyHandler = new GetExerciseHistoryQueryHandler(_unitOfWork);
            _recordsHandler = new GetPersonalRecordsQueryHandler(_unitOfWork);
        }

        private async Task<UserProfile> CreateProfileAsync(string email = "user@test.com", string nickname = "user")
        {
            var account = await AddAccountAsync(email, "pass", nickname);
            return account.Profile;
        }

        /// <summary>Builds a session, adds the given sets, and completes it unless told otherwise.</summary>
        private async Task<WorkoutSession> AddSessionAsync(
            int userProfileId,
            DateOnly performedOn,
            IEnumerable<(int ExerciseId, int Reps, decimal? Weight, WorkoutSetTypeEnum SetType)> sets,
            int durationMinutes = 60,
            WorkoutSessionStatusEnum status = WorkoutSessionStatusEnum.Completed)
        {
            var startedAt = _fixedTestInstant.ToDateTimeUtc();
            var session = WorkoutSession.Start(userProfileId, "Trening", performedOn, startedAt);

            foreach (var group in sets.GroupBy(s => s.ExerciseId))
            {
                var exercise = await _context.Exercises.FindAsync(group.Key);
                var entry = session.AddExercise(exercise!);

                foreach (var set in group)
                    entry.AddSet(startedAt, reps: set.Reps, weight: set.Weight, setType: set.SetType);
            }

            if (status == WorkoutSessionStatusEnum.Completed)
                session.Complete(startedAt.AddMinutes(durationMinutes));
            else if (status == WorkoutSessionStatusEnum.Abandoned)
                session.Abandon(startedAt.AddMinutes(durationMinutes));

            session.ClearDomainEvents();
            _context.WorkoutSessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        [Fact]
        public async Task Summary_ShouldRollUpCompletedSessions()
        {
            var profile = await CreateProfileAsync();

            await AddSessionAsync(profile.Id, new DateOnly(2026, 8, 3),
            [
                (BarbellRow, 8, 60m, WorkoutSetTypeEnum.Normal),
                (BarbellRow, 8, 60m, WorkoutSetTypeEnum.Normal),
                (PullUps, 10, null, WorkoutSetTypeEnum.Normal),
            ], durationMinutes: 45);

            await AddSessionAsync(profile.Id, new DateOnly(2026, 8, 10),
            [
                (Bench, 5, 80m, WorkoutSetTypeEnum.Normal),
            ], durationMinutes: 75);

            var result = await _summaryHandler.Handle(
                new GetWorkoutSummaryQuery(profile.Id, From, To), CancellationToken.None);

            result.SessionCount.Should().Be(2);
            result.SetCount.Should().Be(4);
            result.TotalReps.Should().Be(8 + 8 + 10 + 5);
            result.TotalVolume.Should().Be(960m + 400m);   // pull-ups carry no weight, so no volume
            result.TotalDurationSeconds.Should().Be((45 + 75) * 60);
            result.AverageDurationSeconds.Should().Be(60 * 60);
        }

        [Fact]
        public async Task Summary_ShouldExcludeWarmupsFromEveryTotal()
        {
            var profile = await CreateProfileAsync();

            await AddSessionAsync(profile.Id, new DateOnly(2026, 8, 3),
            [
                (BarbellRow, 12, 20m, WorkoutSetTypeEnum.Warmup),
                (BarbellRow, 8, 60m, WorkoutSetTypeEnum.Normal),
            ]);

            var result = await _summaryHandler.Handle(
                new GetWorkoutSummaryQuery(profile.Id, From, To), CancellationToken.None);

            result.SetCount.Should().Be(1);
            result.TotalReps.Should().Be(8);
            result.TotalVolume.Should().Be(480m);
        }

        [Fact]
        public async Task Summary_ShouldIgnoreAbandonedAndInProgressSessions()
        {
            var profile = await CreateProfileAsync();

            await AddSessionAsync(profile.Id, new DateOnly(2026, 8, 3),
                [(BarbellRow, 8, 60m, WorkoutSetTypeEnum.Normal)],
                status: WorkoutSessionStatusEnum.Abandoned);

            await AddSessionAsync(profile.Id, new DateOnly(2026, 8, 4),
                [(BarbellRow, 8, 60m, WorkoutSetTypeEnum.Normal)],
                status: WorkoutSessionStatusEnum.InProgress);

            var result = await _summaryHandler.Handle(
                new GetWorkoutSummaryQuery(profile.Id, From, To), CancellationToken.None);

            // Neither is training you finished; letting them in would make today's numbers move under the user.
            result.SessionCount.Should().Be(0);
            result.TotalVolume.Should().Be(0m);
        }

        [Fact]
        public async Task Summary_ShouldSplitVolumeByMuscleGroup()
        {
            var profile = await CreateProfileAsync();

            await AddSessionAsync(profile.Id, new DateOnly(2026, 8, 3),
            [
                (BarbellRow, 8, 60m, WorkoutSetTypeEnum.Normal),   // Back  → 480
                (Bench, 8, 50m, WorkoutSetTypeEnum.Normal),        // Chest → 400
            ]);

            var result = await _summaryHandler.Handle(
                new GetWorkoutSummaryQuery(profile.Id, From, To), CancellationToken.None);

            result.ByMuscleGroup.Should().HaveCount(2);
            result.ByMuscleGroup[0].MuscleGroup.Should().Be(MuscleGroupEnum.Back);   // ordered by volume
            result.ByMuscleGroup[0].TotalVolume.Should().Be(480m);
            result.ByMuscleGroup[1].MuscleGroup.Should().Be(MuscleGroupEnum.Chest);
        }

        [Fact]
        public async Task Summary_ShouldExcludeSessionsOutsideTheRange()
        {
            var profile = await CreateProfileAsync();

            await AddSessionAsync(profile.Id, new DateOnly(2026, 7, 31),
                [(BarbellRow, 8, 60m, WorkoutSetTypeEnum.Normal)]);

            var result = await _summaryHandler.Handle(
                new GetWorkoutSummaryQuery(profile.Id, From, To), CancellationToken.None);

            result.SessionCount.Should().Be(0);
        }

        [Fact]
        public async Task ExerciseHistory_ShouldReturnOnePointPerSessionWithTheOneRepMax()
        {
            var profile = await CreateProfileAsync();

            await AddSessionAsync(profile.Id, new DateOnly(2026, 8, 3),
                [(BarbellRow, 8, 60m, WorkoutSetTypeEnum.Normal), (BarbellRow, 6, 65m, WorkoutSetTypeEnum.Normal)]);
            await AddSessionAsync(profile.Id, new DateOnly(2026, 8, 10),
                [(BarbellRow, 5, 100m, WorkoutSetTypeEnum.Normal)]);

            var result = await _historyHandler.Handle(
                new GetExerciseHistoryQuery(profile.Id, BarbellRow, From, To), CancellationToken.None);

            result.ExerciseName.Should().Be("Wiosłowanie sztangą");
            result.Points.Should().HaveCount(2);
            result.Points[0].PerformedOn.Should().Be(new DateOnly(2026, 8, 3));
            result.Points[0].TotalVolume.Should().Be(480m + 390m);
            result.Points[0].MaxWeight.Should().Be(65m);

            // Epley: 100 × (1 + 5/30) = 116.67
            result.Points[1].BestEstimatedOneRepMax.Should().Be(116.67m);
        }

        [Fact]
        public async Task ExerciseHistory_ShouldSkipSessionsThatOnlyWarmedUpOnIt()
        {
            var profile = await CreateProfileAsync();

            await AddSessionAsync(profile.Id, new DateOnly(2026, 8, 3),
                [(BarbellRow, 12, 20m, WorkoutSetTypeEnum.Warmup)]);

            var result = await _historyHandler.Handle(
                new GetExerciseHistoryQuery(profile.Id, BarbellRow, From, To), CancellationToken.None);

            result.Points.Should().BeEmpty();
        }

        [Fact]
        public async Task ExerciseHistory_ShouldRejectAnUnknownExercise()
        {
            var profile = await CreateProfileAsync();

            var act = () => _historyHandler.Handle(
                new GetExerciseHistoryQuery(profile.Id, 999_999, From, To), CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task PersonalRecords_ShouldReportAllTimeMaximaPerExercise()
        {
            var profile = await CreateProfileAsync();

            await AddSessionAsync(profile.Id, new DateOnly(2026, 8, 3),
                [(BarbellRow, 8, 60m, WorkoutSetTypeEnum.Normal), (BarbellRow, 12, 40m, WorkoutSetTypeEnum.Normal)]);
            await AddSessionAsync(profile.Id, new DateOnly(2026, 8, 10),
                [(BarbellRow, 5, 100m, WorkoutSetTypeEnum.Normal)]);

            var result = await _recordsHandler.Handle(
                new GetPersonalRecordsQuery(profile.Id), CancellationToken.None);

            var record = result.Single(r => r.ExerciseId == BarbellRow);
            record.ExerciseName.Should().Be("Wiosłowanie sztangą");
            record.MaxWeight.Should().Be(100m);
            record.MaxReps.Should().Be(12);
            record.MaxSetVolume.Should().Be(500m);     // 5 × 100 beats 8 × 60 and 12 × 40
            record.SetCount.Should().Be(3);
            record.LastPerformedOn.Should().Be(new DateOnly(2026, 8, 10));
        }

        [Fact]
        public async Task PersonalRecords_ShouldExcludeWarmups()
        {
            var profile = await CreateProfileAsync();

            await AddSessionAsync(profile.Id, new DateOnly(2026, 8, 3),
                [(BarbellRow, 1, 200m, WorkoutSetTypeEnum.Warmup), (BarbellRow, 8, 60m, WorkoutSetTypeEnum.Normal)]);

            var result = await _recordsHandler.Handle(
                new GetPersonalRecordsQuery(profile.Id), CancellationToken.None);

            result.Single(r => r.ExerciseId == BarbellRow).MaxWeight.Should().Be(60m);
        }

        [Fact]
        public async Task PersonalRecords_ShouldNotLeakAnotherUsersHistory()
        {
            var owner = await CreateProfileAsync("a@test.com", "aaa");
            var stranger = await CreateProfileAsync("b@test.com", "bbb");

            await AddSessionAsync(owner.Id, new DateOnly(2026, 8, 3),
                [(BarbellRow, 8, 60m, WorkoutSetTypeEnum.Normal)]);

            var result = await _recordsHandler.Handle(
                new GetPersonalRecordsQuery(stranger.Id), CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
