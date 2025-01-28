using Domain.Enum;

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
        public Priority Priority { get; set; }
        public int AccountId { get; set; }
    }
}
