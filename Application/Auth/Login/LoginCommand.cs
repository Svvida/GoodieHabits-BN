using MediatR;

namespace Application.Auth.Login
{
    public record LoginCommand(string Login, string Password) : IRequest<LoginResponse>;
}
