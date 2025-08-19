using Application.Common.Interfaces;

namespace Application.Auth.Commands.Register
{
    public record RegisterCommand(string Email, string Password, string? TimeZoneId) : ICommand<RegisterResponse>;
}
