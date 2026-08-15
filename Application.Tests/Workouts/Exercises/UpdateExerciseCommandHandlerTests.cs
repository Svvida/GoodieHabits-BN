using Application.Workouts.Exercises.Commands.SetExerciseArchived;
using Application.Workouts.Exercises.Commands.UpdateExercise;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Workouts.Exercises
{
    public class UpdateExerciseCommandHandlerTests : TestBase<UpdateExerciseCommandHandler>
    {
        private readonly UpdateExerciseCommandHandler _handler;
        private readonly SetExerciseArchivedCommandHandler _archiveHandler;

        public UpdateExerciseCommandHandlerTests()
        {
            _handler = new UpdateExerciseCommandHandler(_unitOfWork, _mapper);
            _archiveHandler = new SetExerciseArchivedCommandHandler(_unitOfWork, _mapper);
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
        public async Task Handle_ShouldUpdateEveryField()
        {
            var profile = await CreateProfileAsync();
            var exercise = await AddExerciseAsync(profile.Id, "Wyciskanie");

            var command = new UpdateExerciseCommand(
                exercise.Id, "Wyciskanie sztangi", ExerciseMetricEnum.RepsAndWeight,
                MuscleGroupEnum.Chest, EquipmentEnum.Barbell, "łopatki ściągnięte", profile.Id);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Name.Should().Be("Wyciskanie sztangi");
            result.Equipment.Should().Be(EquipmentEnum.Barbell);
            result.Note.Should().Be("łopatki ściągnięte");
        }

        [Fact]
        public async Task Handle_ShouldAllowResendingTheSameNameAsANoOp()
        {
            var profile = await CreateProfileAsync();
            var exercise = await AddExerciseAsync(profile.Id, "Wyciskanie");

            // The uniqueness check must exclude the row being edited, or editing a note would be impossible.
            var command = new UpdateExerciseCommand(
                exercise.Id, "Wyciskanie", ExerciseMetricEnum.RepsAndWeight,
                MuscleGroupEnum.Chest, EquipmentEnum.None, "nowa notatka", profile.Id);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Note.Should().Be("nowa notatka");
        }

        [Fact]
        public async Task Handle_ShouldRejectEditingASystemExercise()
        {
            var profile = await CreateProfileAsync();

            var command = new UpdateExerciseCommand(
                1, "Moje pompki", ExerciseMetricEnum.Reps, MuscleGroupEnum.Chest, EquipmentEnum.None, null, profile.Id);

            var act = () => _handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task Handle_ShouldNotFindAnotherUsersExercise()
        {
            var owner = await CreateProfileAsync("a@test.com", "aaa");
            var stranger = await CreateProfileAsync("b@test.com", "bbb");
            var exercise = await AddExerciseAsync(owner.Id, "Wyciskanie");

            var command = new UpdateExerciseCommand(
                exercise.Id, "Przejęte", ExerciseMetricEnum.Reps, MuscleGroupEnum.Chest, EquipmentEnum.None, null, stranger.Id);

            var act = () => _handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handle_ShouldRejectRenamingOntoAnotherOwnedExercise()
        {
            var profile = await CreateProfileAsync();
            await AddExerciseAsync(profile.Id, "Przysiad");
            var second = await AddExerciseAsync(profile.Id, "Przysiad bułgarski");

            var command = new UpdateExerciseCommand(
                second.Id, "Przysiad", ExerciseMetricEnum.RepsAndWeight, MuscleGroupEnum.Quadriceps, EquipmentEnum.None, null, profile.Id);

            var act = () => _handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<ConflictException>();
        }

        [Fact]
        public async Task SetArchived_ShouldHideTheExerciseWithoutDeletingIt()
        {
            var profile = await CreateProfileAsync();
            var exercise = await AddExerciseAsync(profile.Id, "Wyciskanie");

            var result = await _archiveHandler.Handle(
                new SetExerciseArchivedCommand(exercise.Id, true, profile.Id), CancellationToken.None);

            result.IsArchived.Should().BeTrue();
            _context.Exercises.Should().Contain(e => e.Id == exercise.Id);
        }

        [Fact]
        public async Task SetArchived_ShouldRejectASystemExercise()
        {
            var profile = await CreateProfileAsync();

            var act = () => _archiveHandler.Handle(
                new SetExerciseArchivedCommand(1, true, profile.Id), CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>();
        }
    }
}
