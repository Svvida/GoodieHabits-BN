namespace Application.Dtos.Quests
{
    public abstract class BaseUpdateQuestDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Emoji { get; set; }
        public string? Priority { get; set; }
        public HashSet<int> Labels { get; set; } = null!;
    }
}
