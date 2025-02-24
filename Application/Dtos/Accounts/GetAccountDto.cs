namespace Application.Dtos.Accounts
{
    public class GetAccountDto
    {
        public int AccountId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
    }
}