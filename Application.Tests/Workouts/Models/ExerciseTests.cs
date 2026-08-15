using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Workouts.Models
{
    /// <summary>
    /// Phase 1. Pure entity invariants — no DB, per ARCHITECTURE §10's split.
    /// </summary>
    public class ExerciseTests
    {
        [Fact]
        public void Create_ShouldTrimTheNameAndMarkTheExerciseAsOwned()
        {
            var exercise = Exercise.Create(
                1, "  Wyciskanie sztangi  ", ExerciseMetricEnum.RepsAndWeight, MuscleGroupEnum.Chest, EquipmentEnum.Barbell);

            exercise.Name.Should().Be("Wyciskanie sztangi");
            exercise.UserProfileId.Should().Be(1);
            exercise.IsSystem.Should().BeFalse();
            exercise.IsArchived.Should().BeFalse();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_ShouldRejectABlankName(string name)
        {
            var act = () => Exercise.Create(1, name, ExerciseMetricEnum.Reps);

            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void Create_ShouldRejectANameOverTheLimit()
        {
            var act = () => Exercise.Create(1, new string('x', Exercise.NameMaxLength + 1), ExerciseMetricEnum.Reps);

            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void Create_ShouldRejectAMissingOwner()
        {
            var act = () => Exercise.Create(0, "Przysiad", ExerciseMetricEnum.RepsAndWeight);

            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void CreateSystem_ShouldProduceAnUnownedRowWithAnExplicitId()
        {
            var exercise = Exercise.CreateSystem(42, "Podciąganie", ExerciseMetricEnum.Reps, MuscleGroupEnum.Back);

            exercise.Id.Should().Be(42);
            exercise.UserProfileId.Should().BeNull();
            exercise.IsSystem.Should().BeTrue();
        }

        [Fact]
        public void SetArchived_ShouldRetireTheExerciseWithoutDeletingIt()
        {
            var exercise = Exercise.Create(1, "Przysiad", ExerciseMetricEnum.RepsAndWeight);

            exercise.SetArchived(true);

            exercise.IsArchived.Should().BeTrue();
            exercise.Name.Should().Be("Przysiad");
        }

        [Fact]
        public void ChangeMetricType_ShouldNotAffectAlreadyLoggedHistory()
        {
            var exercise = Exercise.Create(1, "Plank", ExerciseMetricEnum.Time);
            var entry = WorkoutSessionExercise.CreateFrom(exercise, 0);

            exercise.ChangeMetricType(ExerciseMetricEnum.Reps);

            // The session entry snapshotted the metric, so the old row still renders as a timed exercise.
            entry.MetricType.Should().Be(ExerciseMetricEnum.Time);
            exercise.MetricType.Should().Be(ExerciseMetricEnum.Reps);
        }

        [Fact]
        public void Rename_ShouldNotRewriteHistory()
        {
            var exercise = Exercise.Create(1, "Wyciskanie", ExerciseMetricEnum.RepsAndWeight);
            var entry = WorkoutSessionExercise.CreateFrom(exercise, 0);

            exercise.Rename("Wyciskanie sztangi");

            entry.ExerciseName.Should().Be("Wyciskanie");
        }
    }
}
