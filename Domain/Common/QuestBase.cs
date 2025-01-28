namespace Domain.Common
{
    public abstract class QuestBase : BaseEntity
    {
        public required string Title { get; set; }
        public string? Description { get; set; } = null;
        public bool IsCompleted { get; set; } = false;
        public string? Emoji { get; set; } = null;
    }
}
