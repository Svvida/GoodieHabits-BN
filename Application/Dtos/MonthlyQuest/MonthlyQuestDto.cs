namespace Application.Dtos.MonthlyQuest
{
    public class MonthlyQuestDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Emoji { get; set; }
        public bool IsCompleted { get; set; }
        public string? Priority { get; set; }
        public int StartDay { get; set; }
        public int EndDay { get; set; }
        public string? Type { get; set; } = "Monthly";
    }
}
