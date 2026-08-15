using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Workouts.Models
{
    public class WorkoutRoutineTests
    {
        [Fact]
        public void Create_ShouldTrimTheName()
        {
            var routine = WorkoutRoutine.Create(1, "  Push A  ", "  Klata i barki  ");

            routine.Name.Should().Be("Push A");
            routine.Description.Should().Be("Klata i barki");
            routine.IsArchived.Should().BeFalse();
        }

        [Fact]
        public void Create_ShouldRejectAMissingOwner()
        {
            var act = () => WorkoutRoutine.Create(0, "Push A");

            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void ReplaceExercises_ShouldRenumberOrderFromZeroInTheGivenSequence()
        {
            var routine = WorkoutRoutine.Create(1, "Push A");

            routine.ReplaceExercises(
            [
                WorkoutRoutineExercise.Create(exerciseId: 10, order: 7),
                WorkoutRoutineExercise.Create(exerciseId: 11, order: 7),
                WorkoutRoutineExercise.Create(exerciseId: 12, order: 99),
            ]);

            routine.Exercises.Select(e => e.Order).Should().Equal(0, 1, 2);
            routine.Exercises.Select(e => e.ExerciseId).Should().Equal(10, 11, 12);
        }

        [Fact]
        public void ReplaceExercises_ShouldAcceptAnEmptyList()
        {
            var routine = WorkoutRoutine.Create(1, "Push A");
            routine.ReplaceExercises([WorkoutRoutineExercise.Create(10)]);

            // A routine under construction is not an error.
            routine.ReplaceExercises([]);

            routine.Exercises.Should().BeEmpty();
        }

        [Fact]
        public void RoutineExercise_ShouldRejectATargetOutsideItsBounds()
        {
            var act = () => WorkoutRoutineExercise.Create(10, targetReps: 100_000);

            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void RoutineExercise_ShouldAllowEveryTargetToBeOmitted()
        {
            var item = WorkoutRoutineExercise.Create(10);

            item.TargetSets.Should().BeNull();
            item.TargetReps.Should().BeNull();
            item.TargetWeight.Should().BeNull();
        }
    }
}
