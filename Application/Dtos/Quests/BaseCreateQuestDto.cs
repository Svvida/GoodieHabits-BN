using System.Text.Json.Serialization;

namespace Application.Dtos.Quests
{
    public abstract class BaseCreateQuestDto
    {
        public required string Title { get; set; }
        public string? Description { get; set; } = null;
        public DateTime? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;
        public string? Emoji { get; set; } = null;
        public string? Priority { get; set; } = null;
        [JsonIgnore]
        public int AccountId { get; set; }
    }
}
