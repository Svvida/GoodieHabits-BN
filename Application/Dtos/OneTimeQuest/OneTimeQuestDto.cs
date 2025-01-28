using Domain.Enum;

namespace Application.Dtos.OneTimeQuest
{
    public class OneTimeQuestDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Emoji { get; set; }
        public bool IsCompleted { get; set; }
        public PriorityLevel PriorityLevel { get; set; }
        public int AccountId { get; set; }
    }
}
