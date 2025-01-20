using Domain.Enum;

namespace Domain.Common
{
    public abstract class QuestBase : BaseEntity
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public PriorityLevel Priority { get; set; } = PriorityLevel.None;
        public string? Emoji { get; set; } = null;
    }
}
