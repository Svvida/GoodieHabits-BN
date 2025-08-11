namespace Application.Accounts.Commands.ChangePassword
{
    public record ChangePasswordRequest(string OldPassword, string NewPassword, string ConfirmNewPassword);
}
