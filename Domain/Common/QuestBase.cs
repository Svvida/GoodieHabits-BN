namespace Domain.Common
{
    public abstract class QuestBase : BaseEntity
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public bool IsCompleted { get; set; }
        public string? Emoji { get; set; } = null;
    }
}
