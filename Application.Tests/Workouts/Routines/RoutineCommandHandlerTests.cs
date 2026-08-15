using Application.Workouts.Routines.Commands.CreateRoutine;
using Application.Workouts.Routines.Commands.DeleteRoutine;
using Application.Workouts.Routines.Commands.UpdateRoutine;
using Application.Workouts.Routines.Dtos;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests.Workouts.Routines
{
    /// <summary>
    /// Create / update / delete for routines. The load-bearing test here is
    /// <see cref="Update_ShouldNotReachSessionsAlreadyPerformedFromTheRoutine"/> — the template-versus-record
    /// boundary is the decision the whole module leans on.
    /// </summary>
    public class RoutineCommandHandlerTests : TestBase<CreateRoutineCommandHandler>
    {
        private readonly CreateRoutineCommandHandler _createHandler;
        private readonly UpdateRoutineCommandHandler _updateHandler;
        private readonly DeleteRoutineCommandHandler _deleteHandler;

        public RoutineCommandHandlerTests()
        {
            _createHandler = new CreateRoutineCommandHandler(_unitOfWork, _mapper);
            _updateHandler = new UpdateRoutineCommandHandler(_unitOfWork, _mapper);
            _deleteHandler = new DeleteRoutineCommandHandler(_unitOfWork);
        }

        private async Task<UserProfile> CreateProfileAsync(string email = "user@test.com", string nickname = "user")
        {
            var account = await AddAccountAsync(email, "pass", nickname);
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
        public async Task Create_ShouldOrderExercisesByTheirPositionInTheArray()
        {
            var profile = await CreateProfileAsync();
            var bench = await AddExerciseAsync(profile.Id, "Wyciskanie");
            var flyes = await AddExerciseAsync(profile.Id, "Rozpiętki");

            var command = new CreateRoutineCommand(
                "Push A",
                "Klata i barki",
                [
                    new RoutineExerciseInput(flyes.Id, TargetSets: 3, TargetReps: 12),
                    new RoutineExerciseInput(bench.Id, TargetSets: 3, TargetReps: 8, TargetWeight: 60m),
                ],
                profile.Id);

            var result = await _createHandler.Handle(command, CancellationToken.None);

            result.Name.Should().Be("Push A");
            result.Exercises.Select(e => e.Order).Should().Equal(0, 1);
            result.Exercises.Select(e => e.ExerciseName).Should().Equal("Rozpiętki", "Wyciskanie");
            result.Exercises.Last().TargetWeight.Should().Be(60m);
        }

        [Fact]
        public async Task Create_ShouldReturnTheExerciseNameAndMetricFromTheLibrary()
        {
            var profile = await CreateProfileAsync();

            // System exercise 100 is "Podciąganie nachwytem" (Reps).
            var command = new CreateRoutineCommand("Pull A", null, [new RoutineExerciseInput(100)], profile.Id);

            var result = await _createHandler.Handle(command, CancellationToken.None);

            result.Exercises.Should().ContainSingle();
            result.Exercises[0].ExerciseName.Should().Be("Podciąganie nachwytem");
            result.Exercises[0].MetricType.Should().Be(ExerciseMetricEnum.Reps);
        }

        [Fact]
        public async Task Create_ShouldAcceptAnEmptyExerciseList()
        {
            var profile = await CreateProfileAsync();

            var result = await _createHandler.Handle(
                new CreateRoutineCommand("Szkic", null, [], profile.Id), CancellationToken.None);

            result.Exercises.Should().BeEmpty();
        }

        [Fact]
        public async Task Create_ShouldRejectTheWholeRequestWhenAnyExerciseIsUnknown()
        {
            var profile = await CreateProfileAsync();
            var bench = await AddExerciseAsync(profile.Id, "Wyciskanie");

            var command = new CreateRoutineCommand(
                "Push A", null, [new RoutineExerciseInput(bench.Id), new RoutineExerciseInput(999_999)], profile.Id);

            var act = () => _createHandler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();
            _context.WorkoutRoutines.Should().BeEmpty();
        }

        [Fact]
        public async Task Create_ShouldRejectAnotherUsersExercise()
        {
            var owner = await CreateProfileAsync("a@test.com", "aaa");
            var stranger = await CreateProfileAsync("b@test.com", "bbb");
            var exercise = await AddExerciseAsync(owner.Id, "Wyciskanie");

            var command = new CreateRoutineCommand("Push A", null, [new RoutineExerciseInput(exercise.Id)], stranger.Id);

            var act = () => _createHandler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Create_ShouldRejectADuplicateRoutineName()
        {
            var profile = await CreateProfileAsync();
            await _createHandler.Handle(new CreateRoutineCommand("Push A", null, [], profile.Id), CancellationToken.None);

            var act = () => _createHandler.Handle(
                new CreateRoutineCommand("Push A", null, [], profile.Id), CancellationToken.None);

            await act.Should().ThrowAsync<ConflictException>();
        }

        [Fact]
        public async Task Update_ShouldReplaceTheWholeExerciseListAndRenumber()
        {
            var profile = await CreateProfileAsync();
            var bench = await AddExerciseAsync(profile.Id, "Wyciskanie");
            var flyes = await AddExerciseAsync(profile.Id, "Rozpiętki");

            var created = await _createHandler.Handle(
                new CreateRoutineCommand("Push A", null,
                    [new RoutineExerciseInput(bench.Id), new RoutineExerciseInput(flyes.Id)], profile.Id),
                CancellationToken.None);

            var result = await _updateHandler.Handle(
                new UpdateRoutineCommand(created.Id, "Push B", "krócej", [new RoutineExerciseInput(flyes.Id)], profile.Id),
                CancellationToken.None);

            result.Name.Should().Be("Push B");
            result.Description.Should().Be("krócej");
            result.Exercises.Should().ContainSingle();
            result.Exercises[0].ExerciseName.Should().Be("Rozpiętki");
            result.Exercises[0].Order.Should().Be(0);
        }

        [Fact]
        public async Task Update_ShouldNotReachSessionsAlreadyPerformedFromTheRoutine()
        {
            var profile = await CreateProfileAsync();
            var bench = await AddExerciseAsync(profile.Id, "Wyciskanie");
            var flyes = await AddExerciseAsync(profile.Id, "Rozpiętki");

            var created = await _createHandler.Handle(
                new CreateRoutineCommand("Push A", null,
                    [new RoutineExerciseInput(bench.Id, TargetReps: 8), new RoutineExerciseInput(flyes.Id)], profile.Id),
                CancellationToken.None);

            var routine = await _context.WorkoutRoutines
                .Include(r => r.Exercises).ThenInclude(e => e.Exercise)
                .FirstAsync(r => r.Id == created.Id);

            var session = WorkoutSession.StartFromRoutine(
                profile.Id, routine, new DateOnly(2026, 8, 15), DateTime.UtcNow);
            _context.WorkoutSessions.Add(session);
            await _context.SaveChangesAsync();

            // Rewrite the template completely.
            await _updateHandler.Handle(
                new UpdateRoutineCommand(created.Id, "Push A", null, [], profile.Id), CancellationToken.None);

            var saved = await _context.WorkoutSessions
                .Include(s => s.Exercises)
                .FirstAsync(s => s.Id == session.Id);

            // A routine is a template and a session is a record; the template never writes back.
            saved.Name.Should().Be("Push A");
            saved.Exercises.Should().HaveCount(2);
            saved.Exercises.Select(e => e.ExerciseName).Should().Contain("Wyciskanie");
        }

        [Fact]
        public async Task Delete_ShouldKeepPerformedSessionsAndJustClearTheTemplateLink()
        {
            var profile = await CreateProfileAsync();
            var bench = await AddExerciseAsync(profile.Id, "Wyciskanie");

            var created = await _createHandler.Handle(
                new CreateRoutineCommand("Push A", null, [new RoutineExerciseInput(bench.Id)], profile.Id),
                CancellationToken.None);

            var routine = await _context.WorkoutRoutines
                .Include(r => r.Exercises).ThenInclude(e => e.Exercise)
                .FirstAsync(r => r.Id == created.Id);

            var session = WorkoutSession.StartFromRoutine(
                profile.Id, routine, new DateOnly(2026, 8, 15), DateTime.UtcNow);
            _context.WorkoutSessions.Add(session);
            await _context.SaveChangesAsync();

            await _deleteHandler.Handle(new DeleteRoutineCommand(created.Id, profile.Id), CancellationToken.None);

            var saved = await _context.WorkoutSessions.Include(s => s.Exercises).FirstAsync(s => s.Id == session.Id);

            saved.RoutineId.Should().BeNull();
            saved.Exercises.Should().ContainSingle();
            (await _context.WorkoutRoutines.FindAsync(created.Id)).Should().BeNull();
        }

        [Fact]
        public async Task Delete_ShouldNotFindAnotherUsersRoutine()
        {
            var owner = await CreateProfileAsync("a@test.com", "aaa");
            var stranger = await CreateProfileAsync("b@test.com", "bbb");

            var created = await _createHandler.Handle(
                new CreateRoutineCommand("Push A", null, [], owner.Id), CancellationToken.None);

            var act = () => _deleteHandler.Handle(
                new DeleteRoutineCommand(created.Id, stranger.Id), CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
