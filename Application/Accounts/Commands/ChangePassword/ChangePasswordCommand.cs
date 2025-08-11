using MediatR;

namespace Application.Accounts.Commands.ChangePassword
{
    public record ChangePasswordCommand(string OldPassword, string NewPassword, string ConfirmNewPassword, int AccountId) : IRequest;
}