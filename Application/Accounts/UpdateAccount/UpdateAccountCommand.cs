using MediatR;

namespace Application.Accounts.UpdateAccount
{
    public record UpdateAccountCommand(string? Login, string Email, string Nickname, string? Bio, int AccountId) : IRequest;
}
