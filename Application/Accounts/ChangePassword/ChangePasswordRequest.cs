namespace Application.Accounts.ChangePassword
{
    public record ChangePasswordRequest(string OldPassword, string NewPassword, string ConfirmNewPassword);
}
