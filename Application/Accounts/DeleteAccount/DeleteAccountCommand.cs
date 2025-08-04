using MediatR;

namespace Application.Accounts.DeleteAccount
{
    public record DeleteAccountCommand(string Password, string ConfirmPassword, int AccountId) : IRequest;
}
