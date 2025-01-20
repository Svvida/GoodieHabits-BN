using Domain.Common;

namespace Domain.Models
{
    public class OneTimeQuest : QuestBase
    {
        public int OneTimeQuestId { get; set; }
        public int AccountId { get; set; }
        public DateTime? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;
        public Account Account { get; set; } = null!;
        public OneTimeQuest() { }
        public OneTimeQuest(
            int oneTimeQuestId,
            int accountId,
            string title,
            string description)
        {
            OneTimeQuestId = oneTimeQuestId;
            AccountId = accountId;
            Title = title;
            Description = description;
        }
    }
}
