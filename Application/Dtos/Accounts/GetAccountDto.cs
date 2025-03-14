namespace Application.Dtos.Accounts
{
    public class GetAccountDto
    {
        public string? Nickname { get; set; }
        public required string Email { get; set; }
        public string? Name { get; set; } = null;
        public string? Surename { get; set; } = null;
        public GetAccountDataDto Data { get; set; } = new();
        public ICollection<GetAccountPreferencesDto> Preferences { get; set; } = [];
    }
}