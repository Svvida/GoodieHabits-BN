using Application.Common.Interfaces;

namespace Application.Accounts.Commands.UpdateAccount
{
    public record UpdateAccountCommand(string? Login, string Email, string Nickname, string? Bio, int AccountId) : ICommand;
}
