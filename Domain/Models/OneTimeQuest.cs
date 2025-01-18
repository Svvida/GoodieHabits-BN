using Domain.Common;

namespace Domain.Models
{
    public class OneTimeQuest : BaseEntity
    {
        public int OneTimeQuestId { get; set; }
        public int AccountId { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Emoji { get; set; }

        public Account Account { get; set; } = null!;
        public OneTimeQuest() { }
        public OneTimeQuest(int oneTimeQuestId,
            int accountId,
            string title,
            string description,
            DateTime? endDate = null,
            string? emoji = null)
        {
            OneTimeQuestId = oneTimeQuestId;
            AccountId = accountId;
            Title = title;
            Description = description;
            EndDate = endDate;
            Emoji = emoji;
        }
    }
}
