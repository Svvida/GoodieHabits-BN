using Domain.Exceptions;

namespace Domain.Models
{
    /// <summary>
    /// One instance of a repeatable quest, identified by the local calendar period it belongs to.
    /// <para>
    /// <see cref="PeriodStart"/> / <see cref="PeriodEnd"/> are calendar dates (no time, no timezone) —
    /// deliberately *not* UTC instants. A period is a fact about the user's calendar ("the 2nd of August
    /// instance"), so it must not shift when the user travels and their profile timezone changes.
    /// Both bounds are inclusive: daily and weekly occurrences span a single day, monthly ones span a
    /// day range inside one month.
    /// </para>
    /// <para>
    /// <see cref="CompletedAt"/> stays a UTC instant — "when the user tapped complete" is a real moment
    /// in time, and it is what time-of-day analytics are built from.
    /// </para>
    /// </summary>
    public class QuestOccurrence
    {
        public int Id { get; set; }
        public int QuestId { get; set; }
        public DateOnly PeriodStart { get; private set; }
        public DateOnly PeriodEnd { get; private set; }
        public bool WasCompleted { get; private set; } = false;
        public DateTime? CompletedAt { get; private set; } = null;

        /// <summary>
        /// True when the completion was recorded after the period had already elapsed (grace-period
        /// backfill). Kept explicit so completion-rate figures stay explainable.
        /// </summary>
        public bool IsBackfilled { get; private set; } = false;

        public Quest Quest { get; private set; } = null!;

        protected QuestOccurrence() { }
        private QuestOccurrence(Quest quest, DateOnly periodStart, DateOnly periodEnd)
        {
            if (periodEnd < periodStart)
                throw new InvalidArgumentException("Occurrence period end cannot be before its start.");

            Quest = quest;
            PeriodStart = periodStart;
            PeriodEnd = periodEnd;
        }

        public static QuestOccurrence Create(Quest quest, DateOnly periodStart, DateOnly periodEnd)
        {
            return new QuestOccurrence(quest, periodStart, periodEnd);
        }

        /// <summary>True when <paramref name="date"/> falls inside this occurrence's period.</summary>
        public bool Covers(DateOnly date) => PeriodStart <= date && date <= PeriodEnd;

        /// <summary>True when the period is entirely in the past relative to <paramref name="today"/>.</summary>
        public bool HasElapsedOn(DateOnly today) => PeriodEnd < today;

        public void MarkAsCompleted(DateTime completedAtUtc, DateOnly today)
        {
            WasCompleted = true;
            CompletedAt = completedAtUtc;
            IsBackfilled = HasElapsedOn(today);
        }

        public void MarkAsIncompleted()
        {
            WasCompleted = false;
            CompletedAt = null;
            IsBackfilled = false;
        }
    }
}
