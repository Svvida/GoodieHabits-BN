namespace Application.Accounts.Commands.DeleteAccount
{
    public record DeleteAccountRequest(string Password, string ConfirmPassword);
}