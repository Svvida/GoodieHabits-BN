using Domain.Enum;

namespace Application.Dtos.OneTimeQuest
{
    public class CreateOneTimeQuestDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Emoji { get; set; }
        public PriorityLevel PriorityLevel { get; set; } = PriorityLevel.None;
        public int AccountId { get; set; } = 1;
    }
}