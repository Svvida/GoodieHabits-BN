using MediatR;

namespace Application.Accounts.ChangePassword
{
    public record ChangePasswordCommand(string OldPassword, string NewPassword, string ConfirmNewPassword, int AccountId) : IRequest;
}