namespace Application.Dtos.OneTimeQuest
{
    public class OneTimeQuestDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Emoji { get; set; }
        public bool IsCompleted { get; set; }
        public string? Priority { get; set; }
        public string? Type { get; set; } = "One-Time";
    }
}
