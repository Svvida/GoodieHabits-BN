using MediatR;

namespace Application.Auth.Commands.Login
{
    public record LoginCommand(string Login, string Password) : IRequest<LoginResponse>;
}
