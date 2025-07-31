using System.Text.Json.Serialization;

namespace Application.QuestLabels.Commands.PatchQuestLabel
{
    public class PatchQuestLabelDto
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string? Value { get; set; }
        public string? BackgroundColor { get; set; }
        [JsonIgnore]
        public int AccountId { get; set; }
    }
}
