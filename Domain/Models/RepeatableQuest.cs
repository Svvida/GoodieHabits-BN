using Domain.Common;

namespace Domain.Models
{
    public class RepeatableQuest : QuestBase
    {
        public int RepeatableQuestId { get; set; }
        public int AccountId { get; set; }
        public DateTime? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;
        public TimeOnly RepeatTime { get; set; }
        public required RepeatInterval RepeatInterval { get; set; }

        public Account Account { get; set; } = null!;
        public RepeatableQuest() { }
        public RepeatableQuest(int recurringQuestId,
            int accountId,
            string title,
            string description,
            TimeOnly repeatTime,
            RepeatInterval repeatInterval)
        {
            RepeatableQuestId = recurringQuestId;
            AccountId = accountId;
            Title = title;
            Description = description;
            RepeatTime = repeatTime;
            RepeatInterval = repeatInterval;
        }
    }
}