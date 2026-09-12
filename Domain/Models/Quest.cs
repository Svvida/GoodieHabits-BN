using Domain.Calculators;
using Domain.Common;
using Domain.Enums;
using Domain.Events.Quests;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Models
{
    /// <summary>
    /// A habit or task the user is accountable for. Three things that used to be tangled together are now
    /// separate: the <see cref="Schedule"/> says which periods exist, the <see cref="Target"/> says what
    /// counts as done inside one, and <see cref="Completions"/> record what the user actually did.
    /// <para>
    /// There is no <c>IsCompleted</c> flag and no <c>NextResetAt</c>. "Is it done?" is derived from the
    /// period covering today (<see cref="IsCompletedOn"/>), which is what removes the background reset job
    /// and the whole class of bugs where a quest stayed completed because nothing had run to clear it.
    /// </para>
    /// </summary>
    public class Quest : EntityBase
    {
        /// <summary>
        /// How many elapsed days a completion may still be dated back to. This is the catch-up window: people
        /// do the habit and forget to tap, and inside this window that is a recording error worth correcting,
        /// not a cheat. Beyond it, rewriting periods would make every streak and rate untrustworthy.
        /// </summary>
        public const int BackfillGraceDays = 2;

        /// <summary>
        /// How far past its target one period may be pushed. Not a rule the user should ever meet — it is a
        /// guard against a client stuck in a retry loop inflating a habit's history.
        /// </summary>
        private const int OverTargetMultiplier = 10;

        public int Id { get; set; }
        public int UserProfileId { get; set; }

        public string Title { get; private set; } = null!;
        public string? Description { get; private set; }
        public PriorityEnum? Priority { get; private set; }
        public string? Emoji { get; private set; }
        public DifficultyEnum? Difficulty { get; private set; }

        /// <summary>Which periods this quest owns.</summary>
        public QuestSchedule Schedule { get; private set; } = null!;

        /// <summary>What counts as done inside one period.</summary>
        public QuestTarget Target { get; private set; } = null!;

        /// <summary>Calendar date the quest becomes active, in the user's local calendar (inclusive).</summary>
        public DateOnly? StartDate { get; private set; }

        /// <summary>Calendar date the quest stops being active, in the user's local calendar (inclusive).</summary>
        public DateOnly? EndDate { get; private set; }

        /// <summary>Time of day the quest is meant to happen — presentation, and the calendar export later.</summary>
        public TimeOnly? ScheduledTime { get; private set; }

        /// <summary>How long it takes. Only consumed by the calendar export, which needs it to emit a timed event.</summary>
        public int? DurationMinutes { get; private set; }

        public DateTime? LastCompletedAt { get; set; }
        public bool WasEverCompleted { get; set; }

        public UserProfile UserProfile { get; set; } = null!;
        public ICollection<Quest_QuestLabel> Quest_QuestLabels { get; set; } = [];
        public ICollection<UserGoal> UserGoal { get; set; } = [];
        public QuestStatistics? Statistics { get; private set; }
        public ICollection<QuestOccurrence> QuestOccurrences { get; private set; } = [];
        public ICollection<QuestCompletion> Completions { get; private set; } = [];

        // EF Core constructor
        protected Quest() { }

        private Quest(
            string title,
            UserProfile userProfile,
            QuestSchedule schedule,
            QuestTarget target,
            DateTime nowUtc,
            string? description,
            PriorityEnum? priority,
            string? emoji,
            DateOnly? startDate,
            DateOnly? endDate,
            DifficultyEnum? difficulty,
            TimeOnly? scheduledTime,
            int? durationMinutes,
            HashSet<int>? labelIds)
        {
            Title = title ?? throw new InvalidArgumentException("Quest title cannot be null or empty.");
            UserProfile = userProfile ?? throw new InvalidArgumentException("User Profile cannot be null.");
            UserProfileId = userProfile.Id;
            Schedule = schedule ?? throw new InvalidArgumentException("Quest schedule cannot be null.");
            Target = target ?? throw new InvalidArgumentException("Quest target cannot be null.");
            Description = description;
            Priority = priority;
            Emoji = emoji;
            Difficulty = difficulty;
            ScheduledTime = scheduledTime;
            DurationMinutes = ValidateDuration(durationMinutes);

            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                throw new InvalidArgumentException("Start date cannot be after the end date.");

            StartDate = startDate;
            EndDate = endDate;

            SetLabels(labelIds);
            SetCreatedAt(nowUtc);

            if (schedule.IsRepeatable)
                Statistics = QuestStatistics.Create(this);
        }

        public static Quest Create(
            string title,
            UserProfile userProfile,
            QuestSchedule schedule,
            QuestTarget target,
            DateTime nowUtc,
            string? description = null,
            PriorityEnum? priority = null,
            string? emoji = null,
            DateOnly? startDate = null,
            DateOnly? endDate = null,
            DifficultyEnum? difficulty = null,
            TimeOnly? scheduledTime = null,
            int? durationMinutes = null,
            HashSet<int>? labelIds = null)
        {
            return new Quest(
                title, userProfile, schedule, target, nowUtc, description, priority, emoji,
                startDate, endDate, difficulty, scheduledTime, durationMinutes, labelIds);
        }

        // ─────────────────────────────── simple edits ───────────────────────────────

        public void UpdateTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new InvalidArgumentException("Quest title cannot be null or empty.");

            Title = title;
        }

        public void UpdateDescription(string? description) => Description = description;

        public void UpdatePriority(PriorityEnum? priority) => Priority = priority;

        public void UpdateEmoji(string? emoji) => Emoji = emoji;

        public void UpdateDifficulty(DifficultyEnum? difficulty) => Difficulty = difficulty;

        public void UpdateScheduledTime(TimeOnly? scheduledTime) => ScheduledTime = scheduledTime;

        public void UpdateDuration(int? durationMinutes) => DurationMinutes = ValidateDuration(durationMinutes);

        public void SetLabels(HashSet<int>? labelIds)
        {
            Quest_QuestLabels = (labelIds is null || labelIds.Count == 0)
                ? []
                : [.. labelIds.Select(labelId => new Quest_QuestLabel(this, labelId))];
        }

        public void UpdateDates(DateOnly? newStartDate, DateOnly? newEndDate)
        {
            if (newStartDate.HasValue && newEndDate.HasValue && newStartDate > newEndDate)
                throw new InvalidArgumentException("Start date cannot be after the end date.");

            StartDate = newStartDate;
            EndDate = newEndDate;
        }

        // ───────────────────────────── schedule & target ────────────────────────────

        /// <summary>
        /// Repoints the quest at a new schedule and drops the pending periods that no longer fit.
        /// <para>
        /// Elapsed periods and any period carrying completions are left untouched — history is not rewritten,
        /// and a period the user has already worked on is theirs. Everything from today forward with nothing
        /// recorded against it is regenerated. This generalizes what the old weekly-quest update did when a
        /// weekday was removed.
        /// </para>
        /// </summary>
        public void UpdateSchedule(QuestSchedule schedule, DateTime nowUtc, DateOnly today)
        {
            ArgumentNullException.ThrowIfNull(schedule);

            if (Schedule == schedule)
                return;

            Schedule = schedule;

            if (Statistics is null && schedule.IsRepeatable)
                Statistics = QuestStatistics.Create(this);

            DropRegenerablePeriodsFrom(today);
            GenerateMissingPeriodsOn(today);
            RecalculateStatistics(today);
        }

        /// <summary>
        /// Changes what counts as done. Elapsed periods keep the target they were created with, so a habit
        /// made harder today does not retroactively fail last month.
        /// </summary>
        public void UpdateTarget(QuestTarget target, DateTime nowUtc, DateOnly today)
        {
            ArgumentNullException.ThrowIfNull(target);

            if (Target == target)
                return;

            Target = target;

            foreach (var period in QuestOccurrences.Where(p => !p.HasElapsedOn(today)))
                period.RetargetTo(ProratedTargetFor(period), nowUtc, today);

            RecalculateStatistics(today);
        }

        /// <summary>
        /// Pending periods that carry nothing the user did, and so can be safely thrown away and rebuilt
        /// under a new schedule.
        /// </summary>
        private void DropRegenerablePeriodsFrom(DateOnly today)
        {
            var disposable = QuestOccurrences
                .Where(p => !p.HasElapsedOn(today) && p.Progress == 0m && p.Completions.Count == 0 && !p.IsSkipped)
                .ToList();

            foreach (var period in disposable)
                QuestOccurrences.Remove(period);
        }

        // ──────────────────────────────── periods ───────────────────────────────────

        /// <summary>Where recurrence counts from and how long the quest lives.</summary>
        private QuestScheduleBounds Bounds => new(StartDate ?? LocalDateOn(CreatedAt), StartDate, EndDate);

        private DayOfWeek WeekStartsOn => UserProfile?.WeekStartsOn ?? DayOfWeek.Monday;

        /// <summary>The period covering the user's local today, if any has been materialized.</summary>
        public QuestOccurrence? CurrentPeriod(DateOnly today) =>
            QuestOccurrences.FirstOrDefault(p => p.Covers(today));

        /// <summary>
        /// The derived replacement for the old <c>IsCompleted</c> column: true when the period the user is
        /// currently in has reached its target.
        /// </summary>
        public bool IsCompletedOn(DateOnly today) => CurrentPeriod(today)?.IsCompleted ?? false;

        /// <summary>True when the quest is due on <paramref name="date"/> at all.</summary>
        public bool IsDueOn(DateOnly date) =>
            QuestPeriodCalculator.PeriodCovering(Schedule, Bounds, WeekStartsOn, date) is not null;

        /// <summary>
        /// The window the quest is due in on <paramref name="date"/> — computed, not stored, so a caller can
        /// render today's progress before anything has been materialized.
        /// </summary>
        public QuestPeriodWindow? PeriodWindowOn(DateOnly date) =>
            QuestPeriodCalculator.PeriodCovering(Schedule, Bounds, WeekStartsOn, date);

        public int InitializePeriods(DateOnly today) => GenerateMissingPeriodsOn(today);

        /// <summary>
        /// Materializes every period from the end of the last one up to the user's local today. Idempotent:
        /// running it twice generates nothing the second time.
        /// </summary>
        public int GenerateMissingPeriodsOn(DateOnly today)
        {
            DateOnly from;

            if (QuestOccurrences.Count > 0)
            {
                var lastPeriodEnd = QuestOccurrences.Max(p => p.PeriodEnd);

                // Checked before advancing, not after: an open-ended quest's period runs to DateOnly.MaxValue,
                // and asking for the day after that throws rather than returning "nothing left to do".
                if (lastPeriodEnd >= today)
                    return 0;

                from = lastPeriodEnd.AddDays(1);
            }
            else
            {
                from = Bounds.EffectiveFrom;

                if (from > today)
                    return 0;
            }

            var windows = QuestPeriodCalculator.PeriodsBetween(Schedule, Bounds, WeekStartsOn, from, today);

            // PeriodStart alone identifies a period — the database enforces the same with a unique index, so
            // in-memory de-duplication here must see every period the quest owns.
            var existingStarts = QuestOccurrences.Select(p => p.PeriodStart).ToHashSet();

            int generated = 0;
            foreach (var window in windows)
            {
                if (!existingStarts.Add(window.Start))
                    continue;

                AddPeriod(window);
                generated++;
            }

            return generated;
        }

        private QuestOccurrence AddPeriod(QuestPeriodWindow window)
        {
            var period = QuestOccurrence.Create(this, window.Start, window.End, ProratedTargetFor(window));
            QuestOccurrences.Add(period);

            return period;
        }

        /// <summary>
        /// A partial period asks for proportionally less. Without this, a "three times a week" quest created
        /// on a Saturday would open its life with a guaranteed failure.
        /// </summary>
        private decimal ProratedTargetFor(QuestPeriodWindow window) =>
            Target.ProratedAmount(window.Days, window.FullDays);

        private decimal ProratedTargetFor(QuestOccurrence period)
        {
            var window = PeriodWindowOn(period.PeriodStart);

            return window.HasValue
                ? ProratedTargetFor(window.Value)
                : Target.Amount;
        }

        /// <summary>
        /// The period covering <paramref name="date"/>, materializing it if nothing has generated it yet.
        /// Returns null when the quest simply is not due then — the caller records that as an off-schedule
        /// completion rather than refusing it.
        /// <para>
        /// Asks the calculator directly instead of running catch-up generation, so backfilling into a gap
        /// works no matter what has or has not been materialized around it.
        /// </para>
        /// </summary>
        private QuestOccurrence? GetOrMaterializePeriodFor(DateOnly date)
        {
            if (QuestOccurrences.FirstOrDefault(p => p.Covers(date)) is QuestOccurrence existing)
                return existing;

            if (PeriodWindowOn(date) is not QuestPeriodWindow window)
                return null;

            // The unique index on (QuestId, PeriodStart) makes a duplicate a hard failure, so this relies on
            // the quest being loaded with all of its occurrences.
            return QuestOccurrences.FirstOrDefault(p => p.PeriodStart == window.Start) ?? AddPeriod(window);
        }

        // ─────────────────────────────── completions ────────────────────────────────

        /// <summary>
        /// Records one act of doing the quest. This is the only write path for progress — the old
        /// complete/uncomplete toggle is gone, and with it the coin farm it enabled.
        /// </summary>
        public QuestCompletionResult AddCompletion(
            DateTime nowUtc,
            DateOnly today,
            decimal? amount = null,
            DateOnly? completedOn = null,
            TimeOnly? localTime = null,
            CompletionSourceEnum source = CompletionSourceEnum.App,
            Guid? clientRequestId = null,
            string? note = null)
        {
            if (clientRequestId is Guid requestId && FindByRequestId(requestId) is QuestCompletion replayed)
            {
                // An offline retry or a double tap. Returning the original keeps the client's view correct
                // without recording the act twice.
                return new QuestCompletionResult(
                    replayed, replayed.Occurrence, WasAlreadyRecorded: true,
                    PeriodCompleted: false, PeriodUncompleted: false,
                    QuestReward.None, GoalAchieved: false, IsFirstEverCompletion: false);
            }

            var countsFor = completedOn ?? today;
            ValidateCompletionDate(countsFor, today);

            decimal recorded = amount ?? 1m;
            if (recorded <= 0)
                throw new InvalidArgumentException("Completion amount must be greater than zero.");

            var period = GetOrMaterializePeriodFor(countsFor);

            if (period is not null)
            {
                EnforceDailyCap(period, countsFor);
                EnforceOverTargetGuard(period, recorded);
            }

            var completion = QuestCompletion.Create(
                quest: this,
                occurrence: period,
                completedOn: countsFor,
                completedAtUtc: nowUtc,
                localTime: localTime,
                amount: recorded,
                isBackfilled: period?.HasElapsedOn(today) ?? countsFor < today,
                source: source,
                clientRequestId: clientRequestId,
                note: note);

            Completions.Add(completion);
            period?.Completions.Add(completion);

            if (period is null)
            {
                // Off-schedule: recorded for the history and the analytics, but it completes nothing and
                // therefore pays nothing.
                return new QuestCompletionResult(
                    completion, null, WasAlreadyRecorded: false,
                    PeriodCompleted: false, PeriodUncompleted: false,
                    QuestReward.None, GoalAchieved: false, IsFirstEverCompletion: false);
            }

            var change = period.ApplyProgress(recorded, nowUtc, today);
            var reward = QuestReward.None;
            bool goalAchieved = false;
            bool firstEver = false;

            if (change == PeriodProgressChange.Completed)
            {
                LastCompletedAt = nowUtc;

                if (!WasEverCompleted)
                {
                    WasEverCompleted = true;
                    firstEver = true;
                }

                goalAchieved = AchieveActiveGoals(nowUtc);
                reward = GrantReward(period, nowUtc, countsFor, goalAchieved, firstEver);
            }

            RecalculateStatistics(today);

            return new QuestCompletionResult(
                completion, period, WasAlreadyRecorded: false,
                PeriodCompleted: change == PeriodProgressChange.Completed,
                PeriodUncompleted: false,
                reward, goalAchieved, firstEver);
        }

        /// <summary>
        /// Undoes one recorded completion. A reward already paid is deliberately never clawed back and never
        /// paid again: mistakes happen, and policing remove-and-re-add would cost more than it protects.
        /// </summary>
        public QuestCompletionResult RemoveCompletion(QuestCompletion completion, DateTime nowUtc, DateOnly today)
        {
            ArgumentNullException.ThrowIfNull(completion);

            if (completion.QuestId != Id && !ReferenceEquals(completion.Quest, this))
                throw new InvalidArgumentException("Completion does not belong to this quest.");

            var period = completion.Occurrence ?? QuestOccurrences.FirstOrDefault(p => p.Id == completion.OccurrenceId);

            Completions.Remove(completion);
            period?.Completions.Remove(completion);

            var change = period?.ApplyProgress(-completion.Amount, nowUtc, today) ?? PeriodProgressChange.None;

            if (change == PeriodProgressChange.Uncompleted)
                UserProfile.RevertQuestPeriodCompletion(Schedule.Unit);

            RecalculateStatistics(today);

            return new QuestCompletionResult(
                completion, period, WasAlreadyRecorded: false,
                PeriodCompleted: false,
                PeriodUncompleted: change == PeriodProgressChange.Uncompleted,
                QuestReward.None, GoalAchieved: false, IsFirstEverCompletion: false);
        }

        /// <summary>Completions already recorded against the given local day.</summary>
        public IEnumerable<QuestCompletion> CompletionsOn(DateOnly date) =>
            Completions.Where(c => c.CompletedOn == date);

        private QuestCompletion? FindByRequestId(Guid requestId) =>
            Completions.FirstOrDefault(c => c.ClientRequestId == requestId);

        private void ValidateCompletionDate(DateOnly countsFor, DateOnly today)
        {
            if (countsFor > today)
                throw new QuestCompletionException("A quest cannot be completed for a future date.", 400);

            if (countsFor < today.AddDays(-BackfillGraceDays))
            {
                throw new QuestCompletionException(
                    $"A completion can only be dated within the last {BackfillGraceDays} days.", 400);
            }
        }

        private void EnforceDailyCap(QuestOccurrence period, DateOnly countsFor)
        {
            if (Target.MaxCompletionsPerDay is not int cap)
                return;

            // Counted on the day, not the period, so backfilling cannot be used to dodge the cap.
            int already = period.Completions.Count(c => c.CompletedOn == countsFor);

            if (already >= cap)
            {
                throw new QuestCompletionException(
                    $"This quest accepts at most {cap} completion(s) per day.");
            }
        }

        private void EnforceOverTargetGuard(QuestOccurrence period, decimal amount)
        {
            if (period.Progress + amount > period.TargetAmount * OverTargetMultiplier)
                throw new QuestCompletionException("This period already has far more progress than it asked for.");
        }

        private bool AchieveActiveGoals(DateTime nowUtc)
        {
            bool achieved = false;

            // The repository loads only active goals, but a goal whose window has closed must not be
            // achieved by a completion that arrives after it.
            foreach (var goal in UserGoal.Where(g => !g.IsAchieved && !g.IsExpired && nowUtc <= g.EndsAt))
            {
                goal.MarkAsAchieved(nowUtc);
                achieved = true;
            }

            return achieved;
        }

        private QuestReward GrantReward(QuestOccurrence period, DateTime nowUtc, DateOnly countsFor, bool goalAchieved, bool firstEver)
        {
            int goalBonusXp = goalAchieved
                ? UserGoal.Where(g => g.IsAchieved && g.AchievedAt == nowUtc).Sum(g => g.XpBonus)
                : 0;

            // Paid once per period, ever. Re-reaching the target after an undo pays nothing.
            if (period.HasBeenRewarded)
            {
                if (goalBonusXp > 0)
                    UserProfile.ApplyQuestPeriodCompletion(QuestReward.None, goalBonusXp, goalAchieved, firstEver, Schedule.Unit);

                return QuestReward.None;
            }

            var reward = QuestRewardCalculator.Calculate(Schedule, Difficulty, Priority, EndDate, countsFor);

            period.MarkRewardGranted(nowUtc, reward.Xp, reward.Coins);
            UserProfile.ApplyQuestPeriodCompletion(reward, goalBonusXp, goalAchieved, firstEver, Schedule.Unit);

            return reward;
        }

        // ──────────────────────────────── statistics ────────────────────────────────

        public void RecalculateStatistics(DateOnly today)
        {
            if (!Schedule.IsRepeatable)
                return;

            Statistics ??= QuestStatistics.Create(this);
            Statistics.UpdateFrom(QuestStatisticsCalculator.Calculate(QuestOccurrences, today, Completions.Count));
        }

        public void Delete() =>
            AddDomainEvent(new QuestDeletedEvent(Id, UserProfileId, WasEverCompleted));

        private static int? ValidateDuration(int? durationMinutes)
        {
            if (durationMinutes is int minutes && (minutes < 1 || minutes > 24 * 60))
                throw new InvalidArgumentException("Duration must be between 1 and 1440 minutes.");

            return durationMinutes;
        }

        private DateOnly LocalDateOn(DateTime instantUtc)
        {
            if (UserProfile is null)
                throw new InvalidArgumentException($"UserProfile must be loaded to resolve local dates for quest {Id}.");

            return UserProfile.LocalDateOn(instantUtc);
        }
    }
}
