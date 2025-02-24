namespace Application.Dtos.Quests
{
    public abstract class BaseUpdateQuestDto
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Emoji { get; set; }
        public string? Priority { get; set; }
    }
}
