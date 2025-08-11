using Application.Accounts.Dtos;

namespace Application.Accounts.Queries.GetWithProfile
{
    public record GetAccountWithProfileResponse(string? Login,
        string Email,
        DateTime JoinDate,
        UserProfileInfoDto Profile,
        ICollection<AccountPreferencesDto> Preferences);
}
