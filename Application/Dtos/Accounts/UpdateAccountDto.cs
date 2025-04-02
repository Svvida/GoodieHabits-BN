namespace Application.Dtos.Accounts
{
    public class UpdateAccountDto
    {
        public string? Login { get; set; }
        public string? Nickname { get; set; }
        public string Email { get; set; } = null!;
        public string? Bio { get; set; }
    }
}
