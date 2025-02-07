using Domain.Common;
using Domain.Enum;

namespace Domain.Models
{
    public class OneTimeQuest : QuestBase
    {
        public OneTimeQuest() : base() { }
        public OneTimeQuest(int id, string title, PriorityEnum? priority, string? description, string? emoji, DateTime? startDate, DateTime? endDate)
            : base(id, title, description, emoji, startDate, endDate, priority) { }
    }
}
