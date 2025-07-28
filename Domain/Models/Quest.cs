using Domain.Common;
using Domain.Enum;
using Domain.Events;
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
        public QuestStatistics? Statistics { get; set; } = null;
        public ICollection<QuestOccurrence> QuestOccurrences { get; set; } = [];

        // EF Core constructor
        protected Quest() { }
        private Quest(
            string title,
            Account account,
            QuestTypeEnum questType,
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

            SetCreatedAt(DateTime.UtcNow);

            if (IsRepeatable())
            {
                Statistics = new QuestStatistics
                {
                    Quest = this
                };
            }

            // Raise domain event for quest creation
            AddDomainEvent(new QuestCreatedEvent(AccountId, title, questType));
        }

        public static Quest Create(
            string title,
            Account account,
            QuestTypeEnum questType,
            string? description = null,
            PriorityEnum? priority = null,
            string? emoji = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            DifficultyEnum? difficulty = null,
            TimeOnly? scheduledTime = null,
            HashSet<int>? labelIds = null)
        {
            return new Quest(title, account, questType, description, priority, emoji,
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
                    occurrence.WasCompleted = true;
                    occurrence.CompletedAt = completionTime;
                }
            }

            int goalsCompleted = 0;
            int xpGained = 0;
            // We don't have to check if UserGoal is expired/achieved here, because we fetch only active goals in the repository
            if (UserGoal?.Count > 0)
            {
                foreach (var goal in UserGoal)
                {
                    goal.IsAchieved = true;
                    goal.AchievedAt = completionTime;
                    xpGained += goal.XpBonus;
                    goalsCompleted++;
                }
            }

            if (shouldAssignRewards)
                xpGained += 10;

            AddDomainEvent(new QuestCompletedEvent(AccountId, xpGained, goalsCompleted, isFirstTimeCompleted, shouldAssignRewards));
        }

        public void Uncomplete()
        {
            IsCompleted = false;

            if (IsRepeatable())
            {
                foreach (var occurrence in QuestOccurrences)
                {
                    occurrence.WasCompleted = false;
                }
            }

            // We don't reset goals since handler prevent uncompleting if quest is active goal

            AddDomainEvent(new QuestUncompletedEvent(AccountId));
        }

        public void AddOccurrence(QuestOccurrence occurrence)
        {
            if (QuestOccurrences.All(o => o.OccurrenceStart != occurrence.OccurrenceStart && o.OccurrenceEnd != occurrence.OccurrenceEnd))
                QuestOccurrences.Add(occurrence);
        }
        public void AddOccurrences(IEnumerable<QuestOccurrence> occurrences)
        {
            foreach (var occurrence in occurrences)
            {
                if (QuestOccurrences.All(o => o.OccurrenceStart != occurrence.OccurrenceStart && o.OccurrenceEnd != occurrence.OccurrenceEnd))
                    QuestOccurrences.Add(occurrence);
            }
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
    }
}
