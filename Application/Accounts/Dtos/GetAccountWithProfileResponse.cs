namespace Application.Accounts.Dtos
{
    public record GetAccountWithProfileResponse(
        string? Login,
        string Email,
        DateTime JoinDate,
        UserProfileInfoDto Profile,
        AccountPreferencesDto Preferences
    );

    public record UserProfileInfoDto(
        string Nickname,
        string? Avatar,
        string? Bio,
        ICollection<BadgeDto> Badges
    );

    public record AccountPreferencesDto(ActiveCosmeticsDto ActiveCosmetics);

    public record ActiveCosmeticsDto(
        string? AvatarFrameUrl,
        PetDto? Pet,
        string? Title,
        NameEffectDto? NameEffect
    );

    public record PetDto(string PetUrl, string? Animation);
    public record NameEffectDto(string EffectStyle, string? ColorHex);
}
