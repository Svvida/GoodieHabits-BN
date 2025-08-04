using MediatR;

namespace Application.Auth.RefreshAccessToken
{
    public record RefreshAccessTokenCommand(string RefreshToken, string? TimeZoneId) : IRequest<RefreshAccessTokenResponse>;
}
