using MediatR;

namespace Application.Auth.Commands.Register
{
    public record RegisterCommand(string Email, string Password, string? TimeZoneId) : IRequest<RegisterResponse>;
}
