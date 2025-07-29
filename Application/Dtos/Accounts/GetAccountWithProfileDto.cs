using Application.Dtos.UserProfile;

namespace Application.Dtos.Accounts
{
    public class GetAccountWithProfileDto
    {
        public string? Login { get; set; }
        public required string Email { get; set; }
        public DateTime JoinDate { get; set; }
        public GetUserProfileInfoDto Profile { get; set; } = new();
        public ICollection<GetAccountPreferencesDto> Preferences { get; set; } = [];
    }
}