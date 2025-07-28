using System.Text.Json.Serialization;

namespace Application.Dtos.Labels
{
    public class CreateQuestLabelDto
    {
        public string Value { get; set; } = null!;
        public string BackgroundColor { get; set; } = null!;
        [JsonIgnore]
        public int AccountId { get; set; }
    }
}
