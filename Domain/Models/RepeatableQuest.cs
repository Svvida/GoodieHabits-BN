using Domain.Common;
using Domain.Enum;

namespace Domain.Models
{
    public class RepeatableQuest : QuestBase
    {
        public int Id { get; set; }
        public DateTime? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;
        public TimeOnly RepeatTime { get; set; }
        public required RepeatInterval RepeatInterval { get; set; }
        public PriorityLevel PriorityLevel { get; set; }
        public Quest Quest { get; set; } = null!;

        public RepeatableQuest() { }
        public RepeatableQuest(int recurringQuestId,
            int accountId,
            string title,
            string description,
            TimeOnly repeatTime,
            RepeatInterval repeatInterval,
            bool isCompleted,
            PriorityLevel priorityLevel)
        {
            Id = recurringQuestId;
            Title = title;
            Description = description;
            RepeatTime = repeatTime;
            RepeatInterval = repeatInterval;
            IsCompleted = isCompleted;
            PriorityLevel = priorityLevel;
        }
    }
}