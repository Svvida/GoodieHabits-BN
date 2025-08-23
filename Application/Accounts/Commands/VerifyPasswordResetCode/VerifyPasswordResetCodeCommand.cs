using Application.Common.Interfaces;

namespace Application.Accounts.Commands.VerifyPasswordResetCode
{
    public record VerifyPasswordResetCodeCommand(string Email, string ResetCode) : ICommand<bool>;
}
