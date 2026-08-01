using Domain.Calculators;
using Domain.Common;
using Domain.Enums;
using Domain.Events.Quests;
using Domain.Exceptions;

namespace Domain.Models
{
    public class Quest : EntityBase
    {
        public int Id { get; set; }
        public int UserProfileId { get; set; }
        public QuestTypeEnum QuestType { get; private set; }
        public string Title { get; private set; } = null!;
        public string? Description { get; private set; } = null;
        public PriorityEnum? Priority { get; private set; } = null;
        public bool IsCompleted { get; set; } = false;
        public string? Emoji { get; private set; } = null;

        /// <summary>
        /// Calendar date the quest becomes active, in the user's local calendar (inclusive).
        /// A calendar fact, not an instant — see <see cref="QuestOccurrence"/>.
        /// </summary>
        public DateOnly? StartDate { get; private set; } = null;

        /// <summary>Calendar date the quest stops being active, in the user's local calendar (inclusive).</summary>
        public DateOnly? EndDate { get; private set; } = null;

        public DateTime? LastCompletedAt { get; set; } = null;
        public DateTime? NextResetAt { get; set; } = null;
        public bool WasEverCompleted { get; set; } = false;
        public DifficultyEnum? Difficulty { get; private set; } = null;
        public TimeOnly? ScheduledTime { get; private set; } = null;

        public UserProfile UserProfile { get; set; } = null!;
        public ICollection<Quest_QuestLabel> Quest_QuestLabels { get; set; } = [];
        public MonthlyQuest_Days? MonthlyQuest_Days { get; set; } = null;
        public ICollection<WeeklyQuest_Day> WeeklyQuest_Days { get; set; } = [];
        public SeasonalQuest_Season? SeasonalQuest_Season { get; set; } = null;
        public ICollection<UserGoal> UserGoal { get; set; } = [];
        public QuestStatistics? Statistics { get; private set; } = null;
        public ICollection<QuestOccurrence> QuestOccurrences { get; private set; } = [];

        // EF Core constructor
        protected Quest() { }
        private Quest(
            string title,
            UserProfile userProfile,
            QuestTypeEnum questType,
            DateTime nowUtc,
            string? description = null,
            PriorityEnum? priority = null,
            string? emoji = null,
            DateOnly? startDate = null,
            DateOnly? endDate = null,
            DifficultyEnum? difficulty = null,
            TimeOnly? scheduledTime = null,
            HashSet<int>? labelIds = null)
        {
            Title = title ?? throw new InvalidArgumentException("Quest title cannot be null or empty.");
            UserProfile = userProfile ?? throw new InvalidArgumentException("User Profile cannot be null.");
            UserProfileId = userProfile.Id;
            QuestType = questType;
            Description = description;
            Priority = priority;
            Emoji = emoji;
            Difficulty = difficulty;
            ScheduledTime = scheduledTime;

            // Validate date logic
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                throw new InvalidArgumentException("Start date cannot be after the end date.");

            StartDate = startDate;
            EndDate = endDate;

            SetLabels(labelIds);

            SetCreatedAt(nowUtc);

            if (IsRepeatable())
            {
                Statistics = QuestStatistics.Create(this);
            }
        }

        public static Quest Create(
            string title,
            UserProfile userProfile,
            QuestTypeEnum questType,
            DateTime nowUtc,
            string? description = null,
            PriorityEnum? priority = null,
            string? emoji = null,
            DateOnly? startDate = null,
            DateOnly? endDate = null,
            DifficultyEnum? difficulty = null,
            TimeOnly? scheduledTime = null,
            HashSet<int>? labelIds = null)
        {
            return new Quest(title, userProfile, questType, nowUtc, description, priority, emoji,
                           startDate, endDate, difficulty, scheduledTime, labelIds);
        }

        public void UpdateDescription(string? description)
        {
            Description = description;
        }

        public void UpdatePriority(PriorityEnum? priority)
        {
            Priority = priority;
        }

        public void UpdateEmoji(string? emoji)
        {
            Emoji = emoji;
        }

        public void UpdateDifficulty(DifficultyEnum? difficulty)
        {
            Difficulty = difficulty;
        }

        public void UpdateScheduledTime(TimeOnly? scheduledTime)
        {
            ScheduledTime = scheduledTime;
        }
        public void SetNextResetAt(DateTime nowUtc)
        {
            NextResetAt = NextResetDateCalculator.Calculate(this, nowUtc);
        }

        public void UpdateDates(DateOnly? newStartDate, DateOnly? newEndDate)
        {
            if (newStartDate.HasValue && newEndDate.HasValue)
            {
                StartDate = newStartDate;
                EndDate = newEndDate;
            }
            if (newStartDate.HasValue && EndDate.HasValue && newStartDate > EndDate)
                throw new InvalidArgumentException("Start date cannot be after the end date.");

            if (newEndDate.HasValue && StartDate.HasValue && newEndDate < StartDate)
                throw new InvalidArgumentException("End date cannot be before the start date.");

            StartDate = newStartDate;
            EndDate = newEndDate;
        }

        public bool IsRepeatable()
        {
            return QuestType switch
            {
                QuestTypeEnum.Daily => true,
                QuestTypeEnum.Weekly => true,
                QuestTypeEnum.Monthly => true,
                _ => false
            };
        }

        public void Complete(DateTime nowUtc, bool shouldAssignRewards)
        {
            if (IsCompleted)
                return;

            if (IsRepeatable())
            {
                var today = LocalToday(nowUtc);
                NextResetAt = NextResetDateCalculator.Calculate(this, nowUtc);

                var occurrenceToComplete = GetOrCreateCurrentOccurrence(today)
                    ?? throw new NoOccurrenceToMarkAsCompletedException(Id);

                occurrenceToComplete.MarkAsCompleted(nowUtc, today);

                RecalculateStatistics(nowUtc);
            }

            IsCompleted = true;
            LastCompletedAt = nowUtc;

            bool isFirstTimeCompleted = false;
            if (!WasEverCompleted)
            {
                WasEverCompleted = true;
                isFirstTimeCompleted = true;
            }

            bool isGoalCompleted = false;
            int xpGained = 0;
            // We don't have to check if UserGoal is expired/achieved here, because we fetch only active goals in the repository
            if (UserGoal?.Count > 0)
            {
                foreach (var goal in UserGoal)
                {
                    goal.MarkAsAchieved(nowUtc);
                    xpGained += goal.XpBonus;
                    // We are just changing flag since its impossible to complete multiple goals at once but UserGoals is collection
                    isGoalCompleted = true;
                }
            }

            if (shouldAssignRewards)
            {
                int baseXp = 10;
                int bonusXp = CalculateXpModifiers(nowUtc);
                xpGained += (baseXp + bonusXp);
            }

            UserProfile.ApplyQuestCompletionRewards(xpGained, isGoalCompleted, isFirstTimeCompleted, shouldAssignRewards, QuestType);
        }

        // Helper method to calculate dynamic XP based on properties
        private int CalculateXpModifiers(DateTime nowUtc)
        {
            int bonus = 0;

            // 1. Difficulty Bonus
            // Harder tasks = More XP. Adjust these values based on your game balance.
            if (Difficulty.HasValue)
            {
                bonus += Difficulty.Value switch
                {
                    DifficultyEnum.Easy => 0,       // Base XP is enough
                    DifficultyEnum.Medium => 5,     // 1.5x Base
                    DifficultyEnum.Hard => 10,      // 2x Base
                    DifficultyEnum.Impossible => 20, // 3x Base
                    _ => 0
                };
            }

            // 2. Priority Bonus
            // Higher Priority = More XP (Incentivize doing the important stuff)
            if (Priority.HasValue)
            {
                bonus += Priority.Value switch
                {
                    PriorityEnum.Low => 0,
                    PriorityEnum.Medium => 5,
                    PriorityEnum.High => 10,
                    _ => 0
                };
            }

            // 3. Timeliness Bonus (End Date)
            // If EndDate exists AND we are completing it on or before that local calendar day
            if (EndDate.HasValue && LocalToday(nowUtc) <= EndDate.Value)
            {
                // "On Time" bonus. 
                // You can make this flat (e.g., 5 XP) or a multiplier.
                bonus += 5;
            }

            return bonus;
        }

        public void Uncomplete(DateTime utcNow)
        {
            if (!IsCompleted)
                return;
            IsCompleted = false;

            if (IsRepeatable())
            {
                var today = LocalToday(utcNow);

                // Prefer the period the user is actually in; fall back to the most recently completed one.
                var occurrenceToRevert = QuestOccurrences.FirstOrDefault(o => o.WasCompleted && o.Covers(today))
                    ?? QuestOccurrences.Where(o => o.WasCompleted).OrderByDescending(o => o.CompletedAt).FirstOrDefault();

                occurrenceToRevert?.MarkAsIncompleted();
                RecalculateStatistics(utcNow);
            }

            // We don't reset goals since handler prevent uncompleting if quest is active goal

            UserProfile.RevertQuestCompletion(QuestType);
        }

        public QuestOccurrence AddOccurrence(DateOnly periodStart, DateOnly periodEnd)
        {
            var occurrence = QuestOccurrence.Create(this, periodStart, periodEnd);
            QuestOccurrences.Add(occurrence);
            return occurrence;
        }

        public void SetLabels(HashSet<int>? labelIds)
        {
            Quest_QuestLabels = (labelIds is null || labelIds.Count == 0)
                ? []
                : [.. labelIds.Select(labelId => new Quest_QuestLabel(this, labelId))];
        }

        public void SetWeekdays(IEnumerable<WeekdayEnum> weekdays)
        {
            if (weekdays is null || !weekdays.Any())
                throw new InvalidArgumentException("At least one weekday must be provided.");

            WeeklyQuest_Days = [.. weekdays
                .Distinct()
                .Select(w => new WeeklyQuest_Day(w))];
        }

        public void SetMonthlyDays(int startDay, int endDay)
        {
            MonthlyQuest_Days = new MonthlyQuest_Days(startDay, endDay);
        }

        public void SetSeason(SeasonEnum season)
        {
            SeasonalQuest_Season = new SeasonalQuest_Season(season);
        }

        public void Delete()
        {
            AddDomainEvent(new QuestDeletedEvent(Id, UserProfileId, IsCompleted, WasEverCompleted));
        }

        public bool ResetCompletedStatus(DateTime nowUtc, DateOnly today)
        {
            if (!IsCompleted || !IsRepeatable())
                return false;
            if (EndDate.HasValue && EndDate.Value < today)
                return false;
            if (!NextResetAt.HasValue || NextResetAt > nowUtc)
                return false;

            IsCompleted = false;
            return true;
        }

        public int GenerateMissingOccurrences(DateTime utcNow) => GenerateMissingOccurrencesOn(LocalToday(utcNow));

        public int GenerateMissingOccurrencesOn(DateOnly today)
        {
            if (!IsActiveOn(today))
                return 0;

            // Resume from the day after the latest period we already have.
            DateOnly fromDate = QuestOccurrences.Count > 0
                ? QuestOccurrences.Max(o => o.PeriodEnd).AddDays(1)
                : StartDate ?? LocalToday(CreatedAt);

            return GenerateAndAddWindows(fromDate, today);
        }

        public void InitializeOccurrences(DateTime utcNow)
        {
            var today = LocalToday(utcNow);

            if (!IsActiveOn(today))
                return;

            GenerateAndAddWindows(StartDate ?? LocalToday(CreatedAt), today);
        }

        private bool IsActiveOn(DateOnly date)
        {
            if (!IsRepeatable())
                return false;
            if (StartDate.HasValue && StartDate.Value > date)
                return false;
            if (EndDate.HasValue && EndDate.Value < date)
                return false;

            return true;
        }

        private int GenerateAndAddWindows(DateOnly fromDate, DateOnly toDate)
        {
            // Never generate periods outside the quest's active range.
            if (StartDate.HasValue && fromDate < StartDate.Value)
                fromDate = StartDate.Value;
            if (EndDate.HasValue && toDate > EndDate.Value)
                toDate = EndDate.Value;

            if (fromDate > toDate)
                return 0;

            var windows = QuestWindowCalculator.GenerateWindows(this, fromDate, toDate);

            // PeriodStart alone identifies a period — the DB enforces the same via a unique index.
            var existingPeriodStarts = QuestOccurrences.Select(qo => qo.PeriodStart).ToHashSet();

            int generatedCount = 0;
            foreach (var window in windows)
            {
                if (!existingPeriodStarts.Add(window.Start))
                    continue;

                AddOccurrence(window.Start, window.End);
                generatedCount++;
            }

            return generatedCount;
        }

        public void RecalculateStatistics(DateTime nowUtc)
        {
            if (!IsRepeatable())
                return;

            Statistics ??= QuestStatistics.Create(this);
            Statistics.UpdateFrom(QuestStatisticsCalculator.Calculate(QuestOccurrences, LocalToday(nowUtc)));
        }

        private QuestOccurrence? GetOrCreateCurrentOccurrence(DateOnly today)
        {
            var current = QuestOccurrences.FirstOrDefault(o => o.Covers(today));
            if (current is not null)
                return current;

            // None yet for today — catch up on any periods the background job hasn't created.
            GenerateMissingOccurrencesOn(today);

            current = QuestOccurrences.FirstOrDefault(o => o.Covers(today));
            if (current is not null)
                return current;

            // Still nothing (e.g. a weekly quest completed the morning after its scheduled day):
            // allow backfilling the most recent period within the grace window.
            var last = QuestOccurrences.OrderByDescending(o => o.PeriodEnd).FirstOrDefault();
            if (last is not null && last.PeriodEnd >= today.AddDays(-BackfillGraceDays))
                return last;

            return null;
        }

        /// <summary>
        /// How many elapsed days a completion may still be attributed backwards to. Occurrences
        /// completed this way are flagged via <see cref="QuestOccurrence.IsBackfilled"/>.
        /// </summary>
        private const int BackfillGraceDays = 1;

        private DateOnly LocalToday(DateTime instantUtc)
        {
            if (UserProfile is null)
                throw new InvalidArgumentException($"UserProfile must be loaded to resolve local dates for quest {Id}.");

            return UserProfile.LocalDateOn(instantUtc);
        }
    }
}