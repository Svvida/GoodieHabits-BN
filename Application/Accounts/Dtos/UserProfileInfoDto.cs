namespace Application.Accounts.Dtos
{
    public record UserProfileInfoDto(string Nickname, string? Avatar, string? Bio, ICollection<BadgeDto> Badges);
}
