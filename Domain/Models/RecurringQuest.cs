using Domain.Common;

namespace Domain.Models
{
    public class RecurringQuest : BaseEntity
    {
        public int RecurringQuestId { get; set; }
        public int AccountId { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public TimeOnly RepeatTime { get; set; }
        public required string RepeatIntervalJson { get; set; }
        public string? Emoji { get; set; }

        public Account Account { get; set; } = null!;
        public RecurringQuest() { }
        public RecurringQuest(int recurringQuestId,
            int accountId,
            string title,
            string description,
            TimeOnly repeatTime,
            string repeatIntervalJson,
            string? emoji = null)
        {
            RecurringQuestId = recurringQuestId;
            AccountId = accountId;
            Title = title;
            Description = description;
            RepeatTime = repeatTime;
            RepeatIntervalJson = repeatIntervalJson;
            Emoji = emoji;
        }
    }
}