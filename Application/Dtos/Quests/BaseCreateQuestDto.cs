using System.Text.Json.Serialization;
using Domain.Enum;

namespace Application.Dtos.Quests
{
    public abstract class BaseCreateQuestDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; } = null;
        public DateTime? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;
        public string? Emoji { get; set; } = null;
        public string? Priority { get; set; } = null;
        public HashSet<int> Labels { get; set; } = [];
        [JsonIgnore]
        public int AccountId { get; set; }
        [JsonIgnore]
        public virtual QuestTypeEnum QuestType { get; set; }
    }
}
