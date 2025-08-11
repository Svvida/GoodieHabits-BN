using MediatR;

namespace Application.Auth.Commands.RefreshAccessToken
{
    public record RefreshAccessTokenCommand(string RefreshToken, string? TimeZoneId) : IRequest<RefreshAccessTokenResponse>;
}
