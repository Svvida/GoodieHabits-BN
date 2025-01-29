using Domain.Common;
using Domain.Enum;

namespace Domain.Models
{
    public class OneTimeQuest : QuestBase
    {
        public int Id { get; set; }
        public DateTime? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;
        public Priority? Priority { get; set; } = null;
        public Quest Quest { get; set; } = null!;
        public OneTimeQuest() { }
        public OneTimeQuest(
            int oneTimeQuestId,
            string title,
            string description,
            bool isCompleted)
        {
            Id = oneTimeQuestId;
            Title = title;
            Description = description;
            IsCompleted = isCompleted;
        }
    }
}
