using Domain.Enum;

namespace Application.Dtos.OneTimeQuest
{
    public class PatchOneTimeQuestDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Emoji { get; set; }
        public bool? IsCompleted { get; set; }
        public PriorityLevel? PriorityLevel { get; set; }
    }
}
