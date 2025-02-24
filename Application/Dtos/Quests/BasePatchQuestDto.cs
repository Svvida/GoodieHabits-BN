namespace Application.Dtos.Quests
{
    public abstract class BasePatchQuestDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Emoji { get; set; }
        public bool? IsCompleted { get; set; }
        public string? Priority { get; set; }
    }
}
