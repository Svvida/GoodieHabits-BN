namespace Application.Accounts.Commands.UpdateAccount
{
    public record UpdateAccountRequest(string? Login, string Email, string Nickname, string? Bio);
}
