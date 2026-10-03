using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Models
{
    /// <summary>What changed on a period when progress was applied to it.</summary>
    public enum PeriodProgressChange
    {
        None,
        Completed,
        Uncompleted
    }

    /// <summary>
    /// One accountability period of a quest, identified by the local calendar days it covers. Streaks,
    /// completion rate, rewards and goals are all judged per period.
    /// <para>
    /// <see cref="PeriodStart"/> / <see cref="PeriodEnd"/> are calendar dates (no time, no timezone) —
    /// deliberately *not* UTC instants. A period is a fact about the user's calendar ("the 2nd of August
    /// instance"), so it must not shift when the user travels and their profile timezone changes. Both
    /// bounds are inclusive.
    /// </para>
    /// <para>
    /// <see cref="TargetAmount"/> is a <b>snapshot</b> of the quest's target when the period was created.
    /// Editing "twice a week" to "three times a week" must not turn last month's successes into failures —
    /// the same template-versus-record rule as <c>RecurringTransaction → FinanceTransaction</c> and
    /// <c>WorkoutRoutine → WorkoutSession</c>. It is also why periods stay materialized rather than being
    /// recomputed on read: these rows are the historical record of what was asked of the user.
    /// </para>
    /// </summary>
    public class QuestOccurrence
    {
        public int Id { get; set; }
        public int QuestId { get; set; }

        public DateOnly PeriodStart { get; private set; }
        public DateOnly PeriodEnd { get; private set; }

        /// <summary>How much this period asked for — prorated when the period is clipped by the quest's range.</summary>
        public decimal TargetAmount { get; private set; } = 1m;

        /// <summary>Sum of the amounts of this period's completions. Denormalized; <see cref="RowVersion"/> guards it.</summary>
        public decimal Progress { get; private set; }

        /// <summary>When the target was first reached. Null means the period is not complete.</summary>
        public DateTime? CompletedAt { get; private set; }

        /// <summary>
        /// True when the completion was recorded after the period had already elapsed (catch-up backfill).
        /// Kept explicit so completion-rate figures stay explainable.
        /// </summary>
        public bool IsBackfilled { get; private set; }

        /// <summary>
        /// Set once, when the period's reward was paid out. Rewards are per period rather than per tap, so a
        /// "10 glasses of water" habit cannot out-earn a daily gym habit tenfold; recording it here is what
        /// makes the grant idempotent and closes the old complete/uncomplete coin farm.
        /// </summary>
        public DateTime? RewardGrantedAt { get; private set; }

        public int XpAwarded { get; private set; }
        public int CoinsAwarded { get; private set; }

        /// <summary>Phase 2: an excused period. Excluded from rates, and does not break a streak.</summary>
        public DateTime? SkippedAt { get; private set; }
        public string? SkipReason { get; private set; }

        /// <summary>
        /// Concurrency token. <see cref="Progress"/> is denormalized, so without this two simultaneous taps
        /// on a "1 of 2" period would both read 1, both reach the target, and both pay the reward.
        /// </summary>
        public byte[] RowVersion { get; private set; } = [];

        public Quest Quest { get; private set; } = null!;
        public ICollection<QuestCompletion> Completions { get; private set; } = [];

        public bool IsCompleted => CompletedAt.HasValue;

        public bool IsSkipped => SkippedAt.HasValue;

        /// <summary>Remaining amount needed to complete the period; zero once it is done.</summary>
        public decimal Remaining => Math.Max(0m, TargetAmount - Progress);

        protected QuestOccurrence() { }

        private QuestOccurrence(Quest quest, DateOnly periodStart, DateOnly periodEnd, decimal targetAmount)
        {
            if (periodEnd < periodStart)
                throw new InvalidArgumentException("Occurrence period end cannot be before its start.");

            if (targetAmount <= 0)
                throw new InvalidArgumentException("Occurrence target must be greater than zero.");

            Quest = quest;
            PeriodStart = periodStart;
            PeriodEnd = periodEnd;
            TargetAmount = targetAmount;
        }

        public static QuestOccurrence Create(Quest quest, DateOnly periodStart, DateOnly periodEnd, decimal targetAmount)
            => new(quest, periodStart, periodEnd, targetAmount);

        /// <summary>True when <paramref name="date"/> falls inside this period.</summary>
        public bool Covers(DateOnly date) => PeriodStart <= date && date <= PeriodEnd;

        /// <summary>True when the period is entirely in the past relative to <paramref name="today"/>.</summary>
        public bool HasElapsedOn(DateOnly today) => PeriodEnd < today;

        /// <summary>
        /// How this period stands as of the user's local today. The single place the rule lives, so the
        /// statistics cache and the analytics endpoints can never drift apart.
        /// </summary>
        public QuestPeriodOutcomeEnum OutcomeOn(DateOnly today)
        {
            if (IsSkipped)
                return QuestPeriodOutcomeEnum.Skipped;

            if (IsCompleted)
                return QuestPeriodOutcomeEnum.Completed;

            if (!HasElapsedOn(today))
                return QuestPeriodOutcomeEnum.Pending;

            // Elapsed with something recorded is partial credit — still a miss, but the UI can say "1 of 2".
            return Progress > 0m ? QuestPeriodOutcomeEnum.Partial : QuestPeriodOutcomeEnum.Missed;
        }

        /// <summary>
        /// Adds (or, with a negative delta, removes) progress and reports whether that flipped the period's
        /// completed state. Undoing never revokes a reward already paid — see <see cref="RewardGrantedAt"/>.
        /// </summary>
        public PeriodProgressChange ApplyProgress(decimal delta, DateTime nowUtc, DateOnly today)
        {
            bool wasCompleted = IsCompleted;

            Progress = Math.Max(0m, Progress + delta);

            bool isCompleted = Progress >= TargetAmount;

            if (isCompleted && !wasCompleted)
            {
                CompletedAt = nowUtc;
                IsBackfilled = HasElapsedOn(today);
                return PeriodProgressChange.Completed;
            }

            if (!isCompleted && wasCompleted)
            {
                CompletedAt = null;
                IsBackfilled = false;
                return PeriodProgressChange.Uncompleted;
            }

            return PeriodProgressChange.None;
        }

        /// <summary>Records that this period's reward has been paid. Deliberately one-way.</summary>
        public void MarkRewardGranted(DateTime nowUtc, int xp, int coins)
        {
            if (RewardGrantedAt.HasValue)
                return;

            RewardGrantedAt = nowUtc;
            XpAwarded = xp;
            CoinsAwarded = coins;
        }

        public bool HasBeenRewarded => RewardGrantedAt.HasValue;

        /// <summary>
        /// Re-points the period at a new target. Only ever called for periods that have not elapsed —
        /// rewriting an elapsed period's target would rewrite history.
        /// </summary>
        public void RetargetTo(decimal targetAmount, DateTime nowUtc, DateOnly today)
        {
            if (targetAmount <= 0)
                throw new InvalidArgumentException("Occurrence target must be greater than zero.");

            TargetAmount = targetAmount;

            // The new target may be already met, or no longer met, by the progress already recorded.
            ApplyProgress(0m, nowUtc, today);
        }

        public void Skip(DateTime nowUtc, string? reason)
        {
            SkippedAt = nowUtc;
            SkipReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        }

        public void Unskip()
        {
            SkippedAt = null;
            SkipReason = null;
        }
    }
}
