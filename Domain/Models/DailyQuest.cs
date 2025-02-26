using Domain.Common;
using Domain.Enum;

namespace Domain.Models
{
    public class DailyQuest : QuestBase
    {
        public DateTime? LastCompleted { get; set; }
        public DailyQuest() : base() { }
        public DailyQuest(int id, string title, string? description, string? emoji, DateTime? startDate, DateTime? endDate, PriorityEnum? priority)
            : base(id, title, description, emoji, startDate, endDate, priority) { }
    }
}
