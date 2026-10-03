using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Models
{
    /// <summary>
    /// One recorded act of doing the quest — the tap. A period's progress is the sum of these, which is what
    /// makes "brush your teeth twice a day" and "exercise at least twice a week" expressible at all.
    /// <para>
    /// <see cref="CompletedOn"/> is a <see cref="DateOnly"/>: the local day this counts for is a calendar
    /// fact, and it must not move when the user travels and their profile timezone changes. It is also what
    /// makes backfilling work — the client says which day it means instead of the server inferring it from
    /// "now". <see cref="CompletedAt"/> stays the UTC instant of the tap.
    /// </para>
    /// <para>
    /// A null <see cref="OccurrenceId"/> is a legal off-schedule completion (a bonus Tuesday workout on a
    /// Mon/Wed/Fri quest). Same argument as <see cref="SupplementIntake"/>: a model that can only record
    /// planned activity punishes the deviation most worth recording. It counts towards totals and time-of-day
    /// analytics, never towards a period, and never earns a reward.
    /// </para>
    /// </summary>
    public class QuestCompletion : EntityBase
    {
        public const int NoteMaxLength = 250;

        public int Id { get; set; }
        public int QuestId { get; private set; }

        /// <summary>Denormalized owner, so range queries and ownership checks need no join through the quest.</summary>
        public int UserProfileId { get; private set; }

        public int? OccurrenceId { get; private set; }

        /// <summary>The local calendar day this completion counts for.</summary>
        public DateOnly CompletedOn { get; private set; }

        /// <summary>The UTC instant of the tap.</summary>
        public DateTime CompletedAt { get; private set; }

        /// <summary>
        /// The user's local wall-clock time at the tap, snapshotted. Deriving it later from
        /// <see cref="CompletedAt"/> and the profile's *current* timezone would shift the hour of every tap
        /// made while travelling — the same trap <see cref="CompletedOn"/> avoids. Null for rows imported
        /// from before this existed.
        /// </summary>
        public TimeOnly? LocalTime { get; private set; }

        public decimal Amount { get; private set; }

        /// <summary>True when recorded after the period it belongs to had already elapsed.</summary>
        public bool IsBackfilled { get; private set; }

        public CompletionSourceEnum Source { get; private set; } = CompletionSourceEnum.App;

        /// <summary>
        /// Client-supplied idempotency key, unique per quest. Makes an accidental double-tap, an offline
        /// retry and a future calendar sync all safe. This matters more than it used to: once a second tap is
        /// legitimate, a duplicated request would silently complete the day.
        /// </summary>
        public Guid? ClientRequestId { get; private set; }

        public string? Note { get; private set; }

        public Quest Quest { get; set; } = null!;
        public UserProfile UserProfile { get; set; } = null!;
        public QuestOccurrence? Occurrence { get; set; }

        /// <summary>True when this completion belongs to no period — done on a day nothing was due.</summary>
        public bool IsOffSchedule => OccurrenceId is null && Occurrence is null;

        protected QuestCompletion() { }

        private QuestCompletion(
            Quest quest,
            QuestOccurrence? occurrence,
            DateOnly completedOn,
            DateTime completedAtUtc,
            TimeOnly? localTime,
            decimal amount,
            bool isBackfilled,
            CompletionSourceEnum source,
            Guid? clientRequestId,
            string? note)
        {
            if (amount <= 0)
                throw new InvalidArgumentException("Completion amount must be greater than zero.");

            if (note is not null && note.Trim().Length > NoteMaxLength)
                throw new InvalidArgumentException($"Note cannot exceed {NoteMaxLength} characters.");

            Quest = quest;
            QuestId = quest.Id;
            UserProfileId = quest.UserProfileId;
            Occurrence = occurrence;
            OccurrenceId = occurrence?.Id;
            CompletedOn = completedOn;
            CompletedAt = completedAtUtc;
            LocalTime = localTime;
            Amount = amount;
            IsBackfilled = isBackfilled;
            Source = source;
            ClientRequestId = clientRequestId;
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();

            SetCreatedAt(completedAtUtc);
        }

        public static QuestCompletion Create(
            Quest quest,
            QuestOccurrence? occurrence,
            DateOnly completedOn,
            DateTime completedAtUtc,
            TimeOnly? localTime,
            decimal amount,
            bool isBackfilled,
            CompletionSourceEnum source = CompletionSourceEnum.App,
            Guid? clientRequestId = null,
            string? note = null)
        {
            ArgumentNullException.ThrowIfNull(quest);

            return new QuestCompletion(
                quest, occurrence, completedOn, completedAtUtc, localTime,
                amount, isBackfilled, source, clientRequestId, note);
        }

        /// <summary>
        /// Detaches the completion from its period without deleting it, so a schedule change that drops a
        /// period leaves the record of what the user actually did. Mirrors
        /// <see cref="SupplementIntake.DetachFromSlot"/>.
        /// </summary>
        public void DetachFromPeriod()
        {
            OccurrenceId = null;
            Occurrence = null;
        }

        public void UpdateNote(string? note)
        {
            if (note is not null && note.Trim().Length > NoteMaxLength)
                throw new InvalidArgumentException($"Note cannot exceed {NoteMaxLength} characters.");

            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        }
    }
}
