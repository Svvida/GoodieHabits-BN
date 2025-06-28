using System.Text.Json.Serialization;

namespace Application.Dtos.Auth
{
    public class PasswordConfirmationDto
    {
        [JsonIgnore]
        public int AccountId { get; set; }
        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
    }
}
