using Application.Common.Interfaces;

namespace Application.Accounts.Commands.DeleteAccount
{
    public record DeleteAccountCommand(string Password, string ConfirmPassword, int AccountId, int UserProfileId) : ICommand;
}
