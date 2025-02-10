namespace Application.Dtos
{
    public abstract class BaseCreateQuestDto
    {
        public required string Title { get; set; }
        public string? Description { get; set; } = null;
        public DateTime? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;
        public string? Emoji { get; set; } = null;
        public string? Priority { get; set; } = null;
        public int AccountId { get; set; } = 1;
    }
}
