using Domain.Enums;
using Domain.Events.Workouts;
using Domain.Exceptions;
using Domain.Models;
using FluentAssertions;

namespace Application.Tests.Workouts.Models
{
    public class WorkoutSessionTests
    {
        private static readonly DateOnly Today = new(2026, 8, 15);
        private static readonly DateTime StartedAt = new(2026, 8, 15, 17, 30, 0, DateTimeKind.Utc);

        private static Exercise BenchPress(int id = 1) =>
            Exercise.CreateSystem(id, "Wyciskanie sztangi", ExerciseMetricEnum.RepsAndWeight, MuscleGroupEnum.Chest);

        private static WorkoutRoutine PushRoutine(int id = 5)
        {
            var routine = WorkoutRoutine.Create(1, "Push A");
            routine.Id = id;

            var first = WorkoutRoutineExercise.Create(1, order: 0, targetSets: 3, targetReps: 8, targetWeight: 60m);
            first.Exercise = BenchPress();

            var second = WorkoutRoutineExercise.Create(2, order: 1, targetSets: 3, targetReps: 12);
            second.Exercise = Exercise.CreateSystem(2, "Rozpiętki", ExerciseMetricEnum.RepsAndWeight, MuscleGroupEnum.Chest);

            routine.ReplaceExercises([first, second]);
            return routine;
        }

        [Fact]
        public void Start_ShouldOpenAnAdHocSessionInProgress()
        {
            var session = WorkoutSession.Start(1, "Trening", Today, StartedAt);

            session.Status.Should().Be(WorkoutSessionStatusEnum.InProgress);
            session.IsInProgress.Should().BeTrue();
            session.RoutineId.Should().BeNull();
            session.CompletedAt.Should().BeNull();
            session.DurationSeconds.Should().BeNull();
        }

        [Fact]
        public void StartFromRoutine_ShouldCopyExercisesInOrderAndSnapshotNameAndMetric()
        {
            var session = WorkoutSession.StartFromRoutine(1, PushRoutine(), Today, StartedAt);

            session.RoutineId.Should().Be(5);
            session.Name.Should().Be("Push A");
            session.Exercises.Select(e => e.Order).Should().Equal(0, 1);
            session.Exercises.Select(e => e.ExerciseName).Should().Equal("Wyciskanie sztangi", "Rozpiętki");
            session.Exercises.Should().OnlyContain(e => e.MetricType == ExerciseMetricEnum.RepsAndWeight);
            session.Exercises.First().TargetWeight.Should().Be(60m);
        }

        [Fact]
        public void StartFromRoutine_ShouldSnapshotSoLaterRoutineEditsCannotReachThePastSession()
        {
            var routine = PushRoutine();
            var session = WorkoutSession.StartFromRoutine(1, routine, Today, StartedAt);

            routine.Rename("Push B");
            routine.ReplaceExercises([]);

            session.Name.Should().Be("Push A");
            session.Exercises.Should().HaveCount(2);
        }

        [Fact]
        public void StartFromRoutine_ShouldRejectAnotherUsersRoutine()
        {
            var act = () => WorkoutSession.StartFromRoutine(999, PushRoutine(), Today, StartedAt);

            act.Should().Throw<ForbiddenException>();
        }

        [Fact]
        public void StartFromRoutine_ShouldRejectARoutineWhoseExercisesWereNotLoaded()
        {
            var routine = WorkoutRoutine.Create(1, "Push A");
            routine.Id = 5;
            routine.ReplaceExercises([WorkoutRoutineExercise.Create(1)]); // Exercise navigation left unloaded

            var act = () => WorkoutSession.StartFromRoutine(1, routine, Today, StartedAt);

            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void AddExercise_ShouldAppendWithTheNextOrder()
        {
            var session = WorkoutSession.Start(1, "Trening", Today, StartedAt);

            session.AddExercise(BenchPress(1));
            session.AddExercise(BenchPress(2));

            session.Exercises.Select(e => e.Order).Should().Equal(0, 1);
        }

        [Fact]
        public void RemoveExercise_ShouldRenumberWhatIsLeft()
        {
            var session = WorkoutSession.Start(1, "Trening", Today, StartedAt);
            var first = session.AddExercise(BenchPress(1));
            session.AddExercise(BenchPress(2));
            session.AddExercise(BenchPress(3));

            session.RemoveExercise(first);

            session.Exercises.Select(e => e.Order).Should().Equal(0, 1);
        }

        [Fact]
        public void ReplaceLog_ShouldBeIdempotentForTheSamePayload()
        {
            var session = WorkoutSession.Start(1, "Trening", Today, StartedAt);

            static WorkoutSessionExercise[] Payload()
            {
                var entry = WorkoutSessionExercise.CreateFrom(BenchPress(), 0);
                entry.AddSet(StartedAt, reps: 8, weight: 60m);
                return [entry];
            }

            session.ReplaceLog(Payload());
            session.ReplaceLog(Payload());

            // Full replacement, not append — a retried bulk sync must not double the log.
            session.Exercises.Should().ContainSingle();
            session.Exercises.Single().Sets.Should().ContainSingle();
        }

        [Fact]
        public void Complete_ShouldFinishTheSessionAndRaiseTheGamificationHook()
        {
            var session = WorkoutSession.StartFromRoutine(1, PushRoutine(), Today, StartedAt);
            session.Exercises.First().AddSet(StartedAt, reps: 8, weight: 60m);
            session.Exercises.First().AddSet(StartedAt, reps: 8, weight: 60m);

            var completedAt = StartedAt.AddMinutes(45);
            session.Complete(completedAt);

            session.Status.Should().Be(WorkoutSessionStatusEnum.Completed);
            session.CompletedAt.Should().Be(completedAt);
            session.DurationSeconds.Should().Be(45 * 60);

            var raised = session.DomainEvents.OfType<WorkoutSessionCompletedEvent>().Single();
            raised.UserProfileId.Should().Be(1);
            raised.PerformedOn.Should().Be(Today);
            raised.ExerciseCount.Should().Be(2);
            raised.SetCount.Should().Be(2);
        }

        [Fact]
        public void Complete_ShouldRejectASecondCompletion()
        {
            var session = WorkoutSession.Start(1, "Trening", Today, StartedAt);
            session.Complete(StartedAt.AddMinutes(30));

            var act = () => session.Complete(StartedAt.AddMinutes(40));

            act.Should().Throw<ConflictException>();
        }

        [Fact]
        public void Complete_ShouldRejectAnEndBeforeTheStart()
        {
            var session = WorkoutSession.Start(1, "Trening", Today, StartedAt);

            var act = () => session.Complete(StartedAt.AddMinutes(-1));

            act.Should().Throw<InvalidArgumentException>();
        }

        [Fact]
        public void Abandon_ShouldNotRaiseTheCompletionEvent()
        {
            var session = WorkoutSession.Start(1, "Trening", Today, StartedAt);

            session.Abandon(StartedAt.AddMinutes(5));

            session.Status.Should().Be(WorkoutSessionStatusEnum.Abandoned);
            session.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public void Abandon_ShouldRejectACompletedSession()
        {
            var session = WorkoutSession.Start(1, "Trening", Today, StartedAt);
            session.Complete(StartedAt.AddMinutes(30));

            var act = () => session.Abandon(StartedAt.AddMinutes(40));

            act.Should().Throw<ConflictException>();
        }

        [Fact]
        public void DetachRoutine_ShouldLetTheTemplateBeDeletedWithoutTouchingTheSession()
        {
            var session = WorkoutSession.StartFromRoutine(1, PushRoutine(), Today, StartedAt);

            session.DetachRoutine();

            session.RoutineId.Should().BeNull();
            session.Name.Should().Be("Push A");
            session.Exercises.Should().HaveCount(2);
        }
    }
}
