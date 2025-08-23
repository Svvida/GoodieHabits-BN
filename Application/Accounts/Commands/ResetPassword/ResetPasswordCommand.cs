using Application.Common.Interfaces;

namespace Application.Accounts.Commands.ResetPassword
{
    public record ResetPasswordCommand(string Email, string ResetCode, string NewPassword, string ConfirmNewPassword) : ICommand;
}
