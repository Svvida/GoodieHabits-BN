using Domain.Common;
using Domain.Enum;
using Domain.Exceptions;

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
    }
}
