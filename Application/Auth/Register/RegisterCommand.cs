using MediatR;

namespace Application.Auth.Register
{
    public record RegisterCommand(string Email, string Password, string? TimeZoneId) : IRequest<RegisterResponseDto>;
}
