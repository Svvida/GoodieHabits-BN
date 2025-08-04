namespace Application.Accounts.UpdateAccount
{
    public record UpdateAccountRequest(string? Login, string Email, string Nickname, string? Bio);
}
