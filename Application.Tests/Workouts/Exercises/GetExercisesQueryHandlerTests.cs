using Application.Workouts.Exercises.Queries.GetExercises;
using Domain.Enums;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Workouts.Exercises
{
    public class GetExercisesQueryHandlerTests : TestBase<GetExercisesQueryHandler>
    {
        private readonly GetExercisesQueryHandler _handler;

        public GetExercisesQueryHandlerTests()
        {
            _handler = new GetExercisesQueryHandler(_unitOfWork, _mapper);
        }

        private async Task<UserProfile> CreateProfileAsync(string email, string nickname)
        {
            var account = await AddAccountAsync(email, "pass", nickname);
            return account.Profile;
        }

        private async Task<Exercise> AddExerciseAsync(
            int userProfileId, string name, MuscleGroupEnum muscle = MuscleGroupEnum.Chest, bool archived = false)
        {
            var exercise = Exercise.Create(userProfileId, name, ExerciseMetricEnum.Reps, muscle);
            exercise.SetArchived(archived);
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();
            return exercise;
        }

        [Fact]
        public async Task Handle_ShouldReturnSystemExercisesAndTheUsersOwn()
        {
            var profile = await CreateProfileAsync("a@test.com", "aaa");
            await AddExerciseAsync(profile.Id, "Moje ćwiczenie");

            var result = await _handler.Handle(
                new GetExercisesQuery(profile.Id, null, null, null, false), CancellationToken.None);

            result.Should().Contain(e => e.Name == "Moje ćwiczenie" && !e.IsSystem);
            result.Should().Contain(e => e.Name == "Pompki klasyczne" && e.IsSystem);
        }

        [Fact]
        public async Task Handle_ShouldNotLeakAnotherUsersExercises()
        {
            var owner = await CreateProfileAsync("a@test.com", "aaa");
            var stranger = await CreateProfileAsync("b@test.com", "bbb");
            await AddExerciseAsync(owner.Id, "Sekretne ćwiczenie");

            var result = await _handler.Handle(
                new GetExercisesQuery(stranger.Id, null, null, null, false), CancellationToken.None);

            result.Should().NotContain(e => e.Name == "Sekretne ćwiczenie");
        }

        [Fact]
        public async Task Handle_ShouldHideArchivedExercisesUnlessAsked()
        {
            var profile = await CreateProfileAsync("a@test.com", "aaa");
            await AddExerciseAsync(profile.Id, "Zarchiwizowane", archived: true);

            var withoutArchived = await _handler.Handle(
                new GetExercisesQuery(profile.Id, null, null, null, false), CancellationToken.None);
            var withArchived = await _handler.Handle(
                new GetExercisesQuery(profile.Id, null, null, null, true), CancellationToken.None);

            withoutArchived.Should().NotContain(e => e.Name == "Zarchiwizowane");
            withArchived.Should().Contain(e => e.Name == "Zarchiwizowane");
        }

        [Fact]
        public async Task Handle_ShouldFilterByMuscleGroup()
        {
            var profile = await CreateProfileAsync("a@test.com", "aaa");

            var result = await _handler.Handle(
                new GetExercisesQuery(profile.Id, MuscleGroupEnum.Cardio, null, null, false), CancellationToken.None);

            result.Should().NotBeEmpty();
            result.Should().OnlyContain(e => e.MuscleGroup == MuscleGroupEnum.Cardio);
        }

        [Fact]
        public async Task Handle_ShouldFilterBySearchTerm()
        {
            var profile = await CreateProfileAsync("a@test.com", "aaa");

            var result = await _handler.Handle(
                new GetExercisesQuery(profile.Id, null, null, "Podciąganie", false), CancellationToken.None);

            result.Should().NotBeEmpty();
            result.Should().OnlyContain(e => e.Name.Contains("Podciąganie"));
        }

        [Fact]
        public async Task Handle_ShouldFilterByMetricType()
        {
            var profile = await CreateProfileAsync("a@test.com", "aaa");

            var result = await _handler.Handle(
                new GetExercisesQuery(profile.Id, null, ExerciseMetricEnum.DistanceAndTime, null, false), CancellationToken.None);

            result.Should().NotBeEmpty();
            result.Should().OnlyContain(e => e.MetricType == ExerciseMetricEnum.DistanceAndTime);
        }
    }
}
