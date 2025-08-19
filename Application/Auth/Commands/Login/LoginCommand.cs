using Application.Common.Interfaces;

namespace Application.Auth.Commands.Login
{
    public record LoginCommand(string Login, string Password) : ICommand<LoginResponse>;
}
