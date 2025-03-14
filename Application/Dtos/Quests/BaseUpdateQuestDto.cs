namespace Application.Dtos.Quests
{
    public abstract class BaseUpdateQuestDto
    {
        public string Title { get; set; } = string.Empty;// Prevents ASP.NET Core default validation errors.
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Emoji { get; set; }
        public string? Priority { get; set; }
    }
}
