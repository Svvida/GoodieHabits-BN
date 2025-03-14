using System.Text.Json.Serialization;
using Application.Dtos.Labels;

namespace Application.Dtos.Quests
{
    public abstract class BaseCreateQuestDto
    {
        public string Title { get; set; } = string.Empty; // Prevents ASP.NET Core default validation errors.
        public string? Description { get; set; } = null;
        public DateTime? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;
        public string? Emoji { get; set; } = null;
        public string? Priority { get; set; } = null;
        public ICollection<CreateQuestLabelDto> Labels { get; set; } = [];
        [JsonIgnore]
        public int AccountId { get; set; }
    }
}
