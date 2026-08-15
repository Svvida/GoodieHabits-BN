using Domain.Common;
using Domain.Enums;
using Domain.Events.Workouts;
using Domain.Exceptions;

namespace Domain.Models
{
    /// <summary>
    /// A performed training session — the aggregate root the whole workouts module writes through.
    /// <para>
    /// <see cref="PerformedOn"/> is a <see cref="DateOnly"/> (SQL <c>date</c>): "which training day was this"
    /// is a fact about the user's calendar, and <c>UserProfile.TimeZone</c> is rewritten on every token
    /// refresh, so deriving the day from a UTC instant makes sessions jump between days when the user travels.
    /// Same reasoning as <c>FinanceTransaction.OccurredOn</c> and <c>QuestOccurrence.PeriodStart</c>.
    /// <see cref="StartedAt"/> / <see cref="CompletedAt"/> stay UTC instants — "when did I start lifting" is a
    /// genuine instant.
    /// </para>
    /// <para>
    /// At most one session per user may be <see cref="WorkoutSessionStatusEnum.InProgress"/>; the database
    /// enforces it with a filtered unique index and the start handler turns the violation into a friendly
    /// conflict. This is a deliberate step back from the module's general flexibility posture — two concurrent
    /// sessions is not a feature, it is a state the client cannot draw.
    /// </para>
    /// </summary>
    public class WorkoutSession : EntityBase
    {
        public const int NameMaxLength = 100;
        public const int NoteMaxLength = 500;

        public int Id { get; set; }
        public int UserProfileId { get; private set; }

        /// <summary>
        /// The template this session was started from, or null for an ad-hoc session. Provenance only — the
        /// session is a complete record on its own and survives the routine being deleted (the delete handler
        /// clears this, exactly as the recurring-template delete clears
        /// <c>FinanceTransaction.RecurringTransactionId</c>).
        /// </summary>
        public int? RoutineId { get; private set; }

        /// <summary>Snapshot of the routine name at start, or a name the user typed. Never re-read from the routine.</summary>
        public string Name { get; private set; } = null!;

        public DateOnly PerformedOn { get; private set; }
        public DateTime StartedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public WorkoutSessionStatusEnum Status { get; private set; }
        public string? Note { get; private set; }

        public UserProfile UserProfile { get; set; } = null!;
        public WorkoutRoutine? Routine { get; set; }
        public ICollection<WorkoutSessionExercise> Exercises { get; set; } = [];
        public ICollection<SupplementIntake> SupplementIntakes { get; set; } = [];

        public bool IsInProgress => Status == WorkoutSessionStatusEnum.InProgress;

        /// <summary>Wall-clock length of the session, or null while it is still running. Computed, never stored.</summary>
        public int? DurationSeconds =>
            CompletedAt is DateTime completed ? (int)(completed - StartedAt).TotalSeconds : null;

        protected WorkoutSession() { }

        private WorkoutSession(
            int userProfileId,
            string name,
            DateOnly performedOn,
            DateTime startedAt,
            int? routineId,
            string? note)
        {
            if (userProfileId <= 0)
                throw new InvalidArgumentException("UserProfileId must be greater than zero.");

            ValidateName(name);
            ValidateNote(note);

            UserProfileId = userProfileId;
            Name = name.Trim();
            PerformedOn = performedOn;
            StartedAt = startedAt;
            RoutineId = routineId;
            Note = note?.Trim();
            Status = WorkoutSessionStatusEnum.InProgress;
        }

        /// <summary>Starts an ad-hoc session — no template, exercises added as the user goes.</summary>
        public static WorkoutSession Start(
            int userProfileId,
            string name,
            DateOnly performedOn,
            DateTime startedAt,
            string? note = null)
            => new(userProfileId, name, performedOn, startedAt, routineId: null, note);

        /// <summary>
        /// Starts a session from a saved routine, copying its exercises and targets in order. The copy is a
        /// snapshot: later edits to the routine never reach this session.
        /// <para>
        /// <paramref name="routine"/> must be loaded with its <c>Exercises</c> and each item's
        /// <c>Exercise</c> — the session snapshots the exercise name and metric, which cannot be read from a
        /// partially loaded graph.
        /// </para>
        /// </summary>
        public static WorkoutSession StartFromRoutine(
            int userProfileId,
            WorkoutRoutine routine,
            DateOnly performedOn,
            DateTime startedAt,
            string? note = null)
        {
            ArgumentNullException.ThrowIfNull(routine);

            if (routine.UserProfileId != userProfileId)
                throw new ForbiddenException("Cannot start a session from a routine owned by another user.");

            var session = new WorkoutSession(userProfileId, routine.Name, performedOn, startedAt, routine.Id, note);

            foreach (var item in routine.Exercises.OrderBy(e => e.Order))
            {
                if (item.Exercise is null)
                    throw new InvalidArgumentException("Routine exercises must be loaded with their Exercise to start a session.");

                session.Exercises.Add(WorkoutSessionExercise.CreateFromTemplate(item, session.Exercises.Count));
            }

            return session;
        }

        /// <summary>Appends an exercise to a running (or already finished) session, snapshotting its name and metric.</summary>
        public WorkoutSessionExercise AddExercise(
            Exercise exercise,
            int? targetSets = null,
            int? targetReps = null,
            decimal? targetWeight = null,
            int? targetDurationSeconds = null,
            decimal? targetDistance = null,
            int? restSeconds = null,
            string? note = null)
        {
            ArgumentNullException.ThrowIfNull(exercise);

            var entry = WorkoutSessionExercise.CreateFrom(
                exercise, Exercises.Count, targetSets, targetReps, targetWeight, targetDurationSeconds, targetDistance, restSeconds, note);

            Exercises.Add(entry);
            return entry;
        }

        public void RemoveExercise(WorkoutSessionExercise entry)
        {
            ArgumentNullException.ThrowIfNull(entry);

            if (!Exercises.Remove(entry))
                throw new NotFoundException("Exercise entry does not belong to this session.");

            Renumber();
        }

        /// <summary>
        /// Replaces the session's whole exercise+set tree, renumbering positions from 0. This is the bulk
        /// logging path (<c>PUT /workouts/sessions/{id}/log</c>), which exists so a phone with no signal in the
        /// gym can sync once at the end. It is full replacement rather than append precisely so that a retry of
        /// the same payload is idempotent.
        /// </summary>
        public void ReplaceLog(IEnumerable<WorkoutSessionExercise> exercises)
        {
            ArgumentNullException.ThrowIfNull(exercises);

            var ordered = exercises.ToList();

            Exercises.Clear();

            for (var i = 0; i < ordered.Count; i++)
            {
                ordered[i].SetOrder(i);
                Exercises.Add(ordered[i]);
            }
        }

        /// <summary>
        /// Finishes the session and raises <see cref="WorkoutSessionCompletedEvent"/>. Nothing consumes that
        /// event yet — it is the gamification hook. There is no central dispatch in <c>SaveChanges</c>, so the
        /// calling handler must publish and clear the events itself (ARCHITECTURE §9).
        /// </summary>
        public void Complete(DateTime completedAtUtc)
        {
            if (Status == WorkoutSessionStatusEnum.Completed)
                throw new ConflictException("Session is already completed.");

            if (completedAtUtc < StartedAt)
                throw new InvalidArgumentException("CompletedAt cannot be earlier than StartedAt.");

            Status = WorkoutSessionStatusEnum.Completed;
            CompletedAt = completedAtUtc;

            AddDomainEvent(new WorkoutSessionCompletedEvent(
                Id,
                UserProfileId,
                PerformedOn,
                Exercises.Count,
                Exercises.Sum(e => e.Sets.Count)));
        }

        /// <summary>
        /// Gives up on a session without finishing it. Kept rather than deleted so the history stays honest,
        /// and deliberately does <em>not</em> raise the completion event.
        /// </summary>
        public void Abandon(DateTime abandonedAtUtc)
        {
            if (Status == WorkoutSessionStatusEnum.Completed)
                throw new ConflictException("A completed session cannot be abandoned.");

            if (abandonedAtUtc < StartedAt)
                throw new InvalidArgumentException("CompletedAt cannot be earlier than StartedAt.");

            Status = WorkoutSessionStatusEnum.Abandoned;
            CompletedAt = abandonedAtUtc;
        }

        public void Rename(string name)
        {
            ValidateName(name);
            Name = name.Trim();
        }

        public void UpdateNote(string? note)
        {
            ValidateNote(note);
            Note = note?.Trim();
        }

        public void UpdatePerformedOn(DateOnly performedOn) => PerformedOn = performedOn;

        /// <summary>Clears the template link so the routine can be deleted without taking its sessions with it.</summary>
        public void DetachRoutine() => RoutineId = null;

        private void Renumber()
        {
            var i = 0;
            foreach (var entry in Exercises.OrderBy(e => e.Order).ToList())
                entry.SetOrder(i++);
        }

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidArgumentException("Session name cannot be null or whitespace.");
            if (name.Trim().Length > NameMaxLength)
                throw new InvalidArgumentException($"Session name cannot exceed {NameMaxLength} characters.");
        }

        private static void ValidateNote(string? note)
        {
            if (note is not null && note.Trim().Length > NoteMaxLength)
                throw new InvalidArgumentException($"Note cannot exceed {NoteMaxLength} characters.");
        }
    }
}
