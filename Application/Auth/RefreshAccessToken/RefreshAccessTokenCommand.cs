using Application.Dtos.Auth;
using MediatR;

namespace Application.Auth.RefreshAccessToken
{
    public record RefreshAccessTokenCommand(string RefreshToken, string? TimeZoneId) : IRequest<RefreshResponseDto>;
}
