using MediatR;

namespace Application.Accounts.Commands.DeleteAccount
{
    public record DeleteAccountCommand(string Password, string ConfirmPassword, int AccountId) : IRequest;
}
