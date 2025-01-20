using Domain.Common;

namespace Domain.Models
{
    public class Account : BaseEntity
    {
        public int AccountId { get; set; }
        public required string Username { get; set; }
        public required string HashPassword { get; set; }
        public required string Email { get; set; }

        public ICollection<OneTimeQuest> OneTimeQuests { get; set; } = new List<OneTimeQuest>();
        public ICollection<RepeatableQuest> RecurringQuests { get; set; } = new List<RepeatableQuest>();
        public ICollection<UserSeasonalQuest> UserSeasonalQuests { get; set; } = new List<UserSeasonalQuest>();

        public Account() { }

        public Account(int accountId, string username, string hashPassword, string email)
        {
            AccountId = accountId;
            Username = username;
            HashPassword = hashPassword;
            Email = email;
        }
    }
}
