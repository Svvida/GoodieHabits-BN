namespace Application.Accounts.DeleteAccount
{
    public record DeleteAccountRequest(string Password, string ConfirmPassword);
}