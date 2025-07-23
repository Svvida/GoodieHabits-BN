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
        public QuestTypeEnum QuestType { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; } = null;
        public PriorityEnum? Priority { get; set; } = null;
        public bool IsCompleted { get; set; } = false;
        public string? Emoji { get; set; } = null;
        public DateTime? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;
        public DateTime? LastCompletedAt { get; set; } = null;
        public DateTime? NextResetAt { get; set; } = null;
        public bool WasEverCompleted { get; set; } = false;
        public DifficultyEnum? Difficulty { get; set; } = null;
        public TimeOnly? ScheduledTime { get; set; } = null;

        public Account Account { get; set; } = null!;
        public ICollection<Quest_QuestLabel> Quest_QuestLabels { get; set; } = [];
        public MonthlyQuest_Days? MonthlyQuest_Days { get; set; } = null;
        public ICollection<WeeklyQuest_Day> WeeklyQuest_Days { get; set; } = [];
        public SeasonalQuest_Season? SeasonalQuest_Season { get; set; } = null;
        public ICollection<UserGoal> UserGoal { get; set; } = [];
        public QuestStatistics? Statistics { get; set; } = null;
        public ICollection<QuestOccurrence> QuestOccurrences { get; set; } = [];

        public Quest() { }
        public Quest(int id, int accountId, QuestTypeEnum questType, string title)
        {
            Id = id;
            AccountId = accountId;
            QuestType = questType;
            Title = title;
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

        public void AddOccurence(QuestOccurrence occurrence)
        {
            if (QuestOccurrences.All(o => o.Id != occurrence.Id))
                QuestOccurrences.Add(occurrence);
        }

        public void Delete()
        {
            AddDomainEvent(new QuestDeletedEvent(Id, AccountId, IsCompleted, WasEverCompleted));
        }
    }
}
