using Application.Workouts.Exercises.Commands.DeleteExercise;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests.Workouts.Exercises
{
    public class DeleteExerciseCommandHandlerTests : TestBase<DeleteExerciseCommandHandler>
    {
        private readonly DeleteExerciseCommandHandler _handler;

        public DeleteExerciseCommandHandlerTests()
        {
            _handler = new DeleteExerciseCommandHandler(_unitOfWork);
        }

        private async Task<UserProfile> CreateProfileAsync()
        {
            var account = await AddAccountAsync("user@test.com", "pass", "user");
            return account.Profile;
        }

        private async Task<Exercise> AddExerciseAsync(int userProfileId, string name)
        {
            var exercise = Exercise.Create(userProfileId, name, ExerciseMetricEnum.RepsAndWeight, MuscleGroupEnum.Chest);
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();
            return exercise;
        }

        [Fact]
        public async Task Handle_ShouldDeleteAnUnusedOwnedExercise()
        {
            var profile = await CreateProfileAsync();
            var exercise = await AddExerciseAsync(profile.Id, "Wyciskanie");

            await _handler.Handle(new DeleteExerciseCommand(exercise.Id, profile.Id), CancellationToken.None);

            (await _context.Exercises.FindAsync(exercise.Id)).Should().BeNull();
        }

        [Fact]
        public async Task Handle_ShouldRejectDeletingASystemExercise()
        {
            var profile = await CreateProfileAsync();

            var act = () => _handler.Handle(new DeleteExerciseCommand(1, profile.Id), CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task Handle_ShouldBlockDeletionWhileARoutineStillPlansIt()
        {
            var profile = await CreateProfileAsync();
            var exercise = await AddExerciseAsync(profile.Id, "Wyciskanie");

            var routine = WorkoutRoutine.Create(profile.Id, "Push A");
            routine.ReplaceExercises([WorkoutRoutineExercise.Create(exercise.Id)]);
            _context.WorkoutRoutines.Add(routine);
            await _context.SaveChangesAsync();

            var act = () => _handler.Handle(new DeleteExerciseCommand(exercise.Id, profile.Id), CancellationToken.None);

            var exception = await act.Should().ThrowAsync<ConflictException>();
            exception.Which.Message.Should().Contain("Push A");
        }

        [Fact]
        public async Task Handle_ShouldKeepSessionHistoryAndJustClearTheLink()
        {
            var profile = await CreateProfileAsync();
            var exercise = await AddExerciseAsync(profile.Id, "Wyciskanie");

            var session = WorkoutSession.Start(profile.Id, "Trening", new DateOnly(2026, 8, 15), DateTime.UtcNow);
            var entry = session.AddExercise(exercise);
            entry.AddSet(DateTime.UtcNow, reps: 8, weight: 60m);
            _context.WorkoutSessions.Add(session);
            await _context.SaveChangesAsync();

            await _handler.Handle(new DeleteExerciseCommand(exercise.Id, profile.Id), CancellationToken.None);

            var savedEntry = await _context.WorkoutSessionExercises
                .Include(e => e.Sets)
                .FirstAsync(e => e.WorkoutSessionId == session.Id);

            // History renders from the snapshot, so deleting the library row costs the user nothing.
            savedEntry.ExerciseId.Should().BeNull();
            savedEntry.ExerciseName.Should().Be("Wyciskanie");
            savedEntry.MetricType.Should().Be(ExerciseMetricEnum.RepsAndWeight);
            savedEntry.Sets.Should().ContainSingle();
        }
    }
}
