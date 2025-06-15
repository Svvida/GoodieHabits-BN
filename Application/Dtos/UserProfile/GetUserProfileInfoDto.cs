namespace Application.Dtos.UserProfile
{
    public class GetUserProfileInfoDto
    {
        public string? Nickname { get; set; }
        public string? Avatar { get; set; }
        public string? Bio { get; set; }
        public ICollection<GetBadgeDto> Badges { get; set; } = [];
    }
}
