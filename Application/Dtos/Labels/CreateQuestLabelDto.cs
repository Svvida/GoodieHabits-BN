using System.Text.Json.Serialization;

namespace Application.Dtos.Labels
{
    public class CreateQuestLabelDto
    {
        // We initialize properties as an empty string to avoid ASP.NET Core's default deserialization errors.
        // This ensures FluentValidation handles validation instead of returning a generic 400 response.
        public string Value { get; set; } = string.Empty;
        public string BackgroundColor { get; set; } = string.Empty;
        [JsonIgnore]
        public int AccountId { get; set; }
    }
}
