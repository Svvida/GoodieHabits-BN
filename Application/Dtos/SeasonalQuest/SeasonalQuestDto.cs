namespace Application.Dtos.SeasonalQuest
{
    public class SeasonalQuestDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Emoji { get; set; }
        public bool IsCompleted { get; set; }
        public string? Priority { get; set; }
        public required string Season { get; set; }
    }
}
