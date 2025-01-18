using Domain.Common;

namespace Domain.Models
{
    public class OneTimeQuest : BaseEntity
    {
        public int OneTimeQuestId { get; set; }
        public int AccountId { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public DateTime? EndDate { get; set; } = null;
        public string? Emoji { get; set; } = null;
        public bool IsImportant { get; set; } = false;

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
