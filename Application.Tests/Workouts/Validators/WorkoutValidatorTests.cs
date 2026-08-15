using Application.Workouts.Exercises.Commands.CreateExercise;
using Application.Workouts.Routines.Commands.CreateRoutine;
using Application.Workouts.Routines.Dtos;
using Domain.Enums;
using Domain.Models;
using Domain.ValueObjects;
using FluentAssertions;

namespace Application.Tests.Workouts.Validators
{
    /// <summary>
    /// Validators need their own tests: handlers are constructed directly in this suite, so
    /// <c>ValidationBehavior</c> never runs (ARCHITECTURE §10).
    /// </summary>
    public class WorkoutValidatorTests
    {
        private readonly CreateExerciseCommandValidator _exerciseValidator = new();
        private readonly CreateRoutineCommandValidator _routineValidator = new();

        private static CreateExerciseCommand Exercise(string name = "Pompki", string? note = null) =>
            new(name, ExerciseMetricEnum.Reps, MuscleGroupEnum.Chest, EquipmentEnum.None, note, 1);

        private static CreateRoutineCommand Routine(
            string name = "Push A", IReadOnlyList<RoutineExerciseInput>? exercises = null) =>
            new(name, null, exercises ?? [], 1);

        [Fact]
        public void ExerciseValidator_ShouldAcceptAValidCommand()
        {
            _exerciseValidator.Validate(Exercise()).IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void ExerciseValidator_ShouldRejectABlankName(string name)
        {
            _exerciseValidator.Validate(Exercise(name)).IsValid.Should().BeFalse();
        }

        [Fact]
        public void ExerciseValidator_ShouldRejectAnOverlongName()
        {
            var result = _exerciseValidator.Validate(Exercise(new string('x', Domain.Models.Exercise.NameMaxLength + 1)));

            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void ExerciseValidator_ShouldRejectAnUndefinedMetric()
        {
            var command = Exercise() with { MetricType = (ExerciseMetricEnum)99 };

            _exerciseValidator.Validate(command).IsValid.Should().BeFalse();
        }

        [Fact]
        public void ExerciseValidator_ShouldRejectAnOverlongNote()
        {
            var result = _exerciseValidator.Validate(
                Exercise(note: new string('x', Domain.Models.Exercise.NoteMaxLength + 1)));

            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void RoutineValidator_ShouldAcceptAnEmptyExerciseList()
        {
            _routineValidator.Validate(Routine()).IsValid.Should().BeTrue();
        }

        [Fact]
        public void RoutineValidator_ShouldRejectANullExerciseList()
        {
            var command = new CreateRoutineCommand("Push A", null, null!, 1);

            _routineValidator.Validate(command).IsValid.Should().BeFalse();
        }

        [Fact]
        public void RoutineValidator_ShouldRejectAnItemWithoutAnExerciseId()
        {
            var result = _routineValidator.Validate(Routine(exercises: [new RoutineExerciseInput(0)]));

            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void RoutineValidator_ShouldRejectATargetOutsideTheSharedLimits()
        {
            var result = _routineValidator.Validate(
                Routine(exercises: [new RoutineExerciseInput(1, TargetReps: WorkoutLimits.MaxReps + 1)]));

            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void RoutineValidator_ShouldRejectAnOverlongItemNote()
        {
            var note = new string('x', WorkoutRoutineExercise.NoteMaxLength + 1);

            var result = _routineValidator.Validate(
                Routine(exercises: [new RoutineExerciseInput(1, Note: note)]));

            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void RoutineValidator_ShouldAcceptItemsWithNoTargetsAtAll()
        {
            var result = _routineValidator.Validate(
                Routine(exercises: [new RoutineExerciseInput(1), new RoutineExerciseInput(2)]));

            result.IsValid.Should().BeTrue();
        }
    }
}
