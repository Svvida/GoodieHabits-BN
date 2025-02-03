using Domain.Common;
using Domain.Enum;

namespace Domain.Models
{
    public class DailyQuest : QuestBase
    {
        public PriorityEnum? Priority { get; set; } = null;
        public DailyQuest() : base() { }
        public DailyQuest(int id, string title, PriorityEnum? priority, string? description, string? emoji, DateTime? startDate, DateTime? endDate)
            : base(id, title, description, emoji, startDate, endDate)
        {
            Priority = priority;
        }
    }
}
