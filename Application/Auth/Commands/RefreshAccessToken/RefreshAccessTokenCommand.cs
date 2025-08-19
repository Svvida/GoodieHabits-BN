using Application.Common.Interfaces;

namespace Application.Auth.Commands.RefreshAccessToken
{
    public record RefreshAccessTokenCommand(string RefreshToken, string? TimeZoneId) : ICommand<RefreshAccessTokenResponse>;
}
