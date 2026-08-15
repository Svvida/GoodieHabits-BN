using Application.Workouts.Exercises.Commands.CreateExercise;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Workouts.Exercises
{
    public class CreateExerciseCommandHandlerTests : TestBase<CreateExerciseCommandHandler>
    {
        private readonly CreateExerciseCommandHandler _handler;

        public CreateExerciseCommandHandlerTests()
        {
            _handler = new CreateExerciseCommandHandler(_unitOfWork, _mapper);
        }

        private async Task<UserProfile> CreateProfileAsync(string email = "user@test.com", string nickname = "user")
        {
            var account = await AddAccountAsync(email, "pass", nickname);
            return account.Profile;
        }

        [Fact]
        public async Task Handle_ShouldCreateAnOwnedExercise()
        {
            var profile = await CreateProfileAsync();

            var command = new CreateExerciseCommand(
                "  Pompki na poręczach  ", ExerciseMetricEnum.Reps, MuscleGroupEnum.Chest, EquipmentEnum.None, null, profile.Id);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Name.Should().Be("Pompki na poręczach");
            result.IsSystem.Should().BeFalse();
            result.IsArchived.Should().BeFalse();
            result.MetricType.Should().Be(ExerciseMetricEnum.Reps);
        }

        [Fact]
        public async Task Handle_ShouldRejectADuplicateNameForTheSameUser()
        {
            var profile = await CreateProfileAsync();
            var command = new CreateExerciseCommand(
                "Wiosłowanie gumą", ExerciseMetricEnum.Reps, MuscleGroupEnum.Back, EquipmentEnum.ResistanceBand, null, profile.Id);

            await _handler.Handle(command, CancellationToken.None);

            var act = () => _handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<ConflictException>();
        }

        [Fact]
        public async Task Handle_ShouldAllowShadowingASystemExerciseName()
        {
            var profile = await CreateProfileAsync();

            // "Pompki klasyczne" is seeded system exercise 1. Uniqueness is scoped to the user's own rows, so a
            // personal variant under the same name is legal.
            var command = new CreateExerciseCommand(
                "Pompki klasyczne", ExerciseMetricEnum.Reps, MuscleGroupEnum.Chest, EquipmentEnum.None, "moja wersja", profile.Id);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Id.Should().NotBe(1);
            result.IsSystem.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ShouldAllowTwoUsersToUseTheSameName()
        {
            var first = await CreateProfileAsync("a@test.com", "aaa");
            var second = await CreateProfileAsync("b@test.com", "bbb");

            await _handler.Handle(
                new CreateExerciseCommand("Podciąganie na ręczniku", ExerciseMetricEnum.Reps, MuscleGroupEnum.Back, EquipmentEnum.None, null, first.Id),
                CancellationToken.None);

            var result = await _handler.Handle(
                new CreateExerciseCommand("Podciąganie na ręczniku", ExerciseMetricEnum.Reps, MuscleGroupEnum.Back, EquipmentEnum.None, null, second.Id),
                CancellationToken.None);

            result.Name.Should().Be("Podciąganie na ręczniku");
        }
    }
}
