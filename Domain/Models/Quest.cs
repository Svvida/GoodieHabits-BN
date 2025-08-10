using Domain.Calculators;
using Domain.Common;
using Domain.Enum;
using Domain.Events.Quests;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Domain.Models
{
    public class Quest : EntityBase
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public QuestTypeEnum QuestType { get; private set; }
        public string Title { get; private set; } = null!;
        public string? Description { get; private set; } = null;
        public PriorityEnum? Priority { get; private set; } = null;
        public bool IsCompleted { get; set; } = false;
        public string? Emoji { get; private set; } = null;
        public DateTime? StartDate { get; private set; } = null;
        public DateTime? EndDate { get; private set; } = null;
        public DateTime? LastCompletedAt { get; set; } = null;
        public DateTime? NextResetAt { get; set; } = null;
        public bool WasEverCompleted { get; set; } = false;
        public DifficultyEnum? Difficulty { get; private set; } = null;
        public TimeOnly? ScheduledTime { get; private set; } = null;

        public Account Account { get; set; } = null!;
        public ICollection<Quest_QuestLabel> Quest_QuestLabels { get; set; } = [];
        public MonthlyQuest_Days? MonthlyQuest_Days { get; set; } = null;
        public ICollection<WeeklyQuest_Day> WeeklyQuest_Days { get; set; } = [];
        public SeasonalQuest_Season? SeasonalQuest_Season { get; set; } = null;
        public ICollection<UserGoal> UserGoal { get; set; } = [];
        public QuestStatistics? Statistics { get; private set; } = null;
        private readonly List<QuestOccurrence> _occurrences = [];
        public IReadOnlyCollection<QuestOccurrence> QuestOccurrences => _occurrences.AsReadOnly();

        // EF Core constructor
        protected Quest() { }
        private Quest(
            string title,
            Account account,
            QuestTypeEnum questType,
            DateTime nowUtc,
            string? description = null,
            PriorityEnum? priority = null,
            string? emoji = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            DifficultyEnum? difficulty = null,
            TimeOnly? scheduledTime = null,
            HashSet<int>? labelIds = null)
        {
            Title = title ?? throw new InvalidArgumentException("Quest title cannot be null or empty.");
            Account = account ?? throw new InvalidArgumentException("Account cannot be null.");
            AccountId = account.Id;
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
                InitializeOccurrences(nowUtc);
            }

            // Raise domain event for quest creation
            AddDomainEvent(new QuestCreatedEvent(AccountId, title, questType));
        }

        public static Quest Create(
            string title,
            Account account,
            QuestTypeEnum questType,
            DateTime nowUtc,
            string? description = null,
            PriorityEnum? priority = null,
            string? emoji = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            DifficultyEnum? difficulty = null,
            TimeOnly? scheduledTime = null,
            HashSet<int>? labelIds = null)
        {
            return new Quest(title, account, questType, nowUtc, description, priority, emoji,
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
        public void SetNextResetAt(IQuestResetService questResetService)
        {
            NextResetAt = questResetService.GetNextResetTimeUtc(this);
        }

        public void UpdateDates(DateTime? newStartDate, DateTime? newEndDate)
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

            StartDate = newStartDate ?? StartDate;
            EndDate = newEndDate ?? EndDate;
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

        public void Complete(DateTime completionTime, IQuestResetService questResetService, bool shouldAssignRewards)
        {
            IsCompleted = true;
            LastCompletedAt = completionTime;

            bool isFirstTimeCompleted = false;
            if (!WasEverCompleted)
            {
                WasEverCompleted = true;
                isFirstTimeCompleted = true;
            }

            if (IsRepeatable())
            {
                NextResetAt = questResetService.GetNextResetTimeUtc(this);
                foreach (var occurrence in QuestOccurrences)
                {
                    occurrence.MarkAsCompleted(completionTime);
                }
            }

            bool isGoalCompleted = false;
            int xpGained = 0;
            // We don't have to check if UserGoal is expired/achieved here, because we fetch only active goals in the repository
            if (UserGoal?.Count > 0)
            {
                foreach (var goal in UserGoal)
                {
                    goal.MarkAsAchieved(completionTime);
                    xpGained += goal.XpBonus;
                    // We are just changing flag since its impossible to complete multiple goals at once but UserGoals is collection
                    isGoalCompleted = true;
                }
            }

            if (shouldAssignRewards)
                xpGained += 10;

            AddDomainEvent(new QuestCompletedEvent(AccountId, xpGained, isGoalCompleted, isFirstTimeCompleted, shouldAssignRewards));
        }

        public void Uncomplete()
        {
            IsCompleted = false;

            if (IsRepeatable())
            {
                foreach (var occurrence in QuestOccurrences)
                {
                    occurrence.MarkAsIncompleted();
                }
            }

            // We don't reset goals since handler prevent uncompleting if quest is active goal

            AddDomainEvent(new QuestUncompletedEvent(AccountId));
        }

        public void AddOccurrence(DateTime start, DateTime end)
        {
            var occurrence = QuestOccurrence.Create(this, start, end);
            _occurrences.Add(occurrence);
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
            AddDomainEvent(new QuestDeletedEvent(Id, AccountId, IsCompleted, WasEverCompleted));
        }

        public bool ResetCompletedStatus(DateTime nowUtc)
        {
            if (!IsCompleted || !IsRepeatable())
                return false;
            if (EndDate.HasValue && EndDate < nowUtc)
                return false;
            if (!NextResetAt.HasValue || NextResetAt > nowUtc)
                return false;

            IsCompleted = false;
            return true;
        }

        public void RecalculateStatistics(IQuestStatisticsCalculator calculator)
        {
            if (!IsRepeatable())
                return;

            var newStats = calculator.Calculate(QuestOccurrences);
            Statistics!.UpdateFrom(newStats);
        }

        public void CompleteOccurrence(int occurrenceId, DateTime utcNow)
        {
            QuestOccurrences.FirstOrDefault(o => o.Id == occurrenceId)?.MarkAsCompleted(utcNow);
        }

        public void UncompleteOccurrence(int occurrenceId)
        {
            QuestOccurrences.FirstOrDefault(o => o.Id == occurrenceId)?.MarkAsIncompleted();
        }

        public int GenerateMissingOccurrences(DateTime utcNow)
        {
            if (!IsRepeatable() || (StartDate.HasValue && StartDate > utcNow) || (EndDate.HasValue && EndDate < utcNow))
                return 0;

            DateTime fromDate = QuestOccurrences.OrderByDescending(o => o.OccurrenceEnd).FirstOrDefault()?.OccurrenceEnd ?? StartDate ?? CreatedAt;

            return GenerateAndAddWindows(fromDate, utcNow);
        }

        public int InitializeOccurrences(DateTime utcNow)
        {
            if (!IsRepeatable() || (StartDate.HasValue && StartDate > utcNow) || (EndDate.HasValue && EndDate < utcNow))
                return 0;

            if (StartDate.HasValue && StartDate > utcNow)
                return 0;

            DateTime fromDate = StartDate ?? CreatedAt;
            return GenerateAndAddWindows(fromDate, utcNow);
        }

        private int GenerateAndAddWindows(DateTime fromDate, DateTime toDate)
        {
            if (fromDate >= toDate)
                return 0;

            var windows = QuestWindowCalculator.GenerateWindows(this, fromDate, toDate);

            int generatedCount = 0;
            foreach (var window in windows)
            {
                AddOccurrence(window.Start, window.End);
                generatedCount++;
            }
            return generatedCount;
        }
    }
}