using Application.Dtos.Profiles;

namespace Application.Dtos.Accounts
{
    public class GetAccountDto
    {
        public string? Login { get; set; }
        public required string Email { get; set; }
        public GetUserProfileDto Profile { get; set; } = new();
        public ICollection<GetAccountPreferencesDto> Preferences { get; set; } = [];
    }
}