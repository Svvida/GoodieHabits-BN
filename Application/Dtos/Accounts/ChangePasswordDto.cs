namespace Application.Dtos.Accounts
{
    public class ChangePasswordDto
    {
        public string OldPassword { get; set; } = null!;
        public string ConfirmOldPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
        public string ConfirmNewPassword { get; set; } = null!;
    }
}
