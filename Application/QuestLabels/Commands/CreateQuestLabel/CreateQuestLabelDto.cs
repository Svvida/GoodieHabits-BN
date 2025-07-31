using System.Text.Json.Serialization;

namespace Application.QuestLabels.Commands.CreateQuestLabel
{
    public class CreateQuestLabelDto
    {
        public string Value { get; set; } = null!;
        public string BackgroundColor { get; set; } = null!;
        [JsonIgnore]
        public int AccountId { get; set; }
    }
}
