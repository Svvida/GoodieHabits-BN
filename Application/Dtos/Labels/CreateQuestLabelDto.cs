using System.Text.Json.Serialization;

namespace Application.Dtos.Labels
{
    public class CreateQuestLabelDto
    {
        public required string Value { get; set; }
        public required string BackgroundColor { get; set; }
        public required string TextColor { get; set; }
        [JsonIgnore]
        public int AccountId { get; set; }
    }
}
