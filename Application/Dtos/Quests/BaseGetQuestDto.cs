using Application.Dtos.Labels;

namespace Application.Dtos.Quests
{
    public abstract class BaseGetQuestDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Emoji { get; set; }
        public bool IsCompleted { get; set; }
        public string? Priority { get; set; }
        public virtual string? Type { get; set; }
        public ICollection<GetQuestLabelDto> QuestLabels { get; set; } = [];
    }
}
