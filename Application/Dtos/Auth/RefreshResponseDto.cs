using System.Text.Json.Serialization;

namespace Application.Dtos.Auth
{
    public class RefreshResponseDto
    {
        public required string AccessToken { get; set; }
        [JsonIgnore]
        public int AccountId { get; set; }
    }
}
